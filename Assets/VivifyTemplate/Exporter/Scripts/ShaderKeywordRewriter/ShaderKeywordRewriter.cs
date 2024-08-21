using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.ShaderKeywordRewriter
{
    public static class ShaderKeywordRewriter
    {
        // Console.WriteLine should be replaced by Debug.Log to be seen, but I didn't want to flood the log
        public static bool Rewrite(string filePath, string targetPath)
        {
            Console.WriteLine($"Loading asset bundle from '{filePath}'");

            AssetsManager manager = new AssetsManager();

            using (FileStream readStream = File.OpenRead(filePath))
            {
                BundleFileInstance bundleInstance = manager.LoadBundleFile(readStream, false);

                Console.WriteLine("Bundle created in Unity " + bundleInstance.file.Header.EngineVersion);

                AssetBundleCompressionType compressionType = AssetBundleCompressionType.None;

                if (bundleInstance.file.DataIsCompressed)
                {
                    compressionType = bundleInstance.file.GetCompressionType();
                    Console.WriteLine($"Decompressing using {compressionType}");
                    bundleInstance.file = BundleHelper.UnpackBundle(bundleInstance.file);
                }

                int fileIndex = 0;
                AssetsFileInstance assetsFileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, fileIndex);
                AssetsFile assetsFile = assetsFileInstance.file;

                // assetsFile.PrintFieldNodes("m_ValidKeywords");
                // assetsFile.PrintFieldNodes("m_InvalidKeywords");

                TypeTreeType typeTreeType = assetsFile.Metadata.FindTypeTreeTypeByID((int)AssetClassID.Material);
                TypeTreeType typeTreeTypeWorkingCopy = new TypeTreeType
                {
                    Nodes = new List<TypeTreeNode>(typeTreeType.Nodes),
                    StringBufferBytes = typeTreeType.StringBufferBytes
                };

                typeTreeTypeWorkingCopy.AppendNode(
                    -1,
                    1,
                    0x8000,
                    "m_ValidKeywords",
                    0,
                    TypeTreeNodeFlags.None,
                    "vector",
                    1);
                typeTreeTypeWorkingCopy.AppendNode(-1, 2, 0xC000, "Array", 0, TypeTreeNodeFlags.Array, "Array", 1);
                typeTreeTypeWorkingCopy.AppendNode(4, 3, 0, "size", 0, TypeTreeNodeFlags.None, "int", 1);
                typeTreeTypeWorkingCopy.AppendNode(-1, 3, 0x8000, "data", 0, TypeTreeNodeFlags.None, "string", 1);
                typeTreeTypeWorkingCopy.AppendNode(-1, 4, 0x4001, "Array", 0, TypeTreeNodeFlags.Array, "Array", 1);
                typeTreeTypeWorkingCopy.AppendNode(4, 5, 0x0001, "size", 0, TypeTreeNodeFlags.None, "int", 1);
                typeTreeTypeWorkingCopy.AppendNode(1, 5, 0x0001, "data", 0, TypeTreeNodeFlags.None, "char", 1);

                typeTreeTypeWorkingCopy.AppendNode(
                    -1,
                    1,
                    0x8000,
                    "m_InvalidKeywords",
                    0,
                    TypeTreeNodeFlags.None,
                    "vector",
                    1);
                typeTreeTypeWorkingCopy.AppendNode(-1, 2, 0xC000, "Array", 0, TypeTreeNodeFlags.Array, "Array", 1);
                typeTreeTypeWorkingCopy.AppendNode(4, 3, 0, "size", 0, TypeTreeNodeFlags.None, "int", 1);
                typeTreeTypeWorkingCopy.AppendNode(-1, 3, 0x8000, "data", 0, TypeTreeNodeFlags.None, "string", 1);
                typeTreeTypeWorkingCopy.AppendNode(-1, 4, 0x4001, "Array", 0, TypeTreeNodeFlags.Array, "Array", 1);
                typeTreeTypeWorkingCopy.AppendNode(4, 5, 0x0001, "size", 0, TypeTreeNodeFlags.None, "int", 1);
                typeTreeTypeWorkingCopy.AppendNode(1, 5, 0x0001, "data", 0, TypeTreeNodeFlags.None, "char", 1);

                bool anyKeywordsUpdated = false;

                Console.WriteLine("Updating materials");

                foreach (AssetFileInfo materialInfo in assetsFile.GetAssetsOfType(AssetClassID.Material))
                {
                    AssetTypeValueField materialBaseField = manager.GetBaseField(assetsFileInstance, materialInfo);

                    Console.WriteLine("-> " + materialBaseField["m_Name"].AsString);

                    materialBaseField.InitializeField(typeTreeTypeWorkingCopy, "m_ValidKeywords");
                    materialBaseField.InitializeField(typeTreeTypeWorkingCopy, "m_InvalidKeywords");

                    string[] shaderKeywords = materialBaseField["m_ShaderKeywords"]
                        .AsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    AssetTypeValueField validKeywordsArray = materialBaseField["m_ValidKeywords.Array"];

                    foreach (string shaderKeyword in shaderKeywords)
                    {
                        Console.WriteLine("--> " + shaderKeyword);

                        AssetTypeValueField arrayValue =
                            ValueBuilder.DefaultValueFieldFromArrayTemplate(validKeywordsArray);
                        arrayValue.AsString = shaderKeyword;
                        validKeywordsArray.Children.Add(arrayValue);
                        anyKeywordsUpdated = true;
                    }

                    materialInfo.SetNewData(materialBaseField);
                }

                if (!anyKeywordsUpdated)
                {
                    Console.WriteLine("No shader keywords found in any materials; no changes needed");
                    return false;
                }

                typeTreeType.Nodes[0].Version =
                    8; // necessary for new fields to be read - doesn't seem to affect loading in 2019
                typeTreeType.Nodes = typeTreeTypeWorkingCopy.Nodes;
                typeTreeType.StringBufferBytes = typeTreeTypeWorkingCopy.StringBufferBytes;

                bundleInstance.file.BlockAndDirInfo.DirectoryInfos[fileIndex].SetNewData(assetsFile);

                Console.WriteLine("Writing updated data");

                string tempPath = Path.GetTempFileName();
                using (FileStream writeStream = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (AssetsFileWriter writer = new AssetsFileWriter(writeStream))
                {
                    // Pack doesn't use content replacers so we need to write uncompressed first
                    bundleInstance.file.Write(writer);
                }

                if (compressionType == AssetBundleCompressionType.None)
                {
                    Debug.Log($"Saving to '{targetPath}'");
                    File.Copy(tempPath, targetPath, true);
                }
                else
                {
                    Debug.Log($"Compressing bundle and saving to '{targetPath}'");
                    AssetBundleFile compressedBundle = new AssetBundleFile();

                    using (FileStream uncompressedReadStream = File.OpenRead(tempPath))
                    using (AssetsFileReader reader = new AssetsFileReader(uncompressedReadStream))
                    {
                        compressedBundle.Read(reader);

                        using (FileStream writeStream = File.Open(
                                   targetPath,
                                   FileMode.Create,
                                   FileAccess.Write,
                                   FileShare.None))
                        using (AssetsFileWriter writer = new AssetsFileWriter(writeStream))
                        {
                            // LZMA is the modern default
                            compressedBundle.Pack(writer, compressionType);
                        }
                    }

                    compressedBundle.Close();
                }

                File.Delete(tempPath);

                return true;
            }
        }
    }
}
