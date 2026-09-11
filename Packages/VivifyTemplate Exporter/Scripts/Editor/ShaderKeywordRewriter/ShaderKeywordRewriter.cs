using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VivifyTemplate.Exporter.Scripts.Editor.Utility;
using Logger = VivifyTemplate.Exporter.Scripts.Editor.Build.Logger;
namespace VivifyTemplate.Exporter.Scripts.Editor.ShaderKeywordRewriter
{
    public static class ShaderKeywordRewriter
    {
        // Adapted from: https://github.com/nicoco007/AssetBundleLoadingTools/blob/shader-keyword-rewriter/ShaderKeywordRewriter/Program.cs
        public static async Task<uint> Rewrite(string filePath, string targetPath, Logger logger, bool compress)
        {
            logger.Log($"Loading asset bundle from '{filePath}'");

            AssetsManager manager = new AssetsManager();

            using (FileStream readStream = File.OpenRead(filePath))
            {
                BundleFileInstance bundleInstance = manager.LoadBundleFile(readStream, false);

                logger.Log("Bundle created in Unity " + bundleInstance.file.Header.EngineVersion);

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

                logger.Log("Updating materials");

                foreach (AssetFileInfo materialInfo in assetsFile.GetAssetsOfType(AssetClassID.Material))
                {
                    AssetTypeValueField materialBaseField = manager.GetBaseField(assetsFileInstance, materialInfo);

                    logger.Log("-> " + materialBaseField["m_Name"].AsString);

                    materialBaseField.InitializeField(typeTreeTypeWorkingCopy, "m_ValidKeywords");
                    materialBaseField.InitializeField(typeTreeTypeWorkingCopy, "m_InvalidKeywords");

                    string[] shaderKeywords = materialBaseField["m_ShaderKeywords"]
                        .AsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    AssetTypeValueField validKeywordsArray = materialBaseField["m_ValidKeywords.Array"];

                    foreach (string shaderKeyword in shaderKeywords)
                    {
                        logger.Log("--> " + shaderKeyword);

                        AssetTypeValueField arrayValue =
                            ValueBuilder.DefaultValueFieldFromArrayTemplate(validKeywordsArray);
                        arrayValue.AsString = shaderKeyword;
                        validKeywordsArray.Children.Add(arrayValue);
                    }

                    materialInfo.SetNewData(materialBaseField);
                }

                typeTreeType.Nodes[0].Version =
                    8; // necessary for new fields to be read - doesn't seem to affect loading in 2019
                typeTreeType.Nodes = typeTreeTypeWorkingCopy.Nodes;
                typeTreeType.StringBufferBytes = typeTreeTypeWorkingCopy.StringBufferBytes;

                bundleInstance.file.BlockAndDirInfo.DirectoryInfos[fileIndex].SetNewData(assetsFile);

                logger.Log("Writing updated data");

                string tempPath = Path.GetTempFileName();
                using (FileStream writeStream = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (AssetsFileWriter writer = new AssetsFileWriter(writeStream))
                {
                    // Pack doesn't use content replacers so we need to write uncompressed first
                    bundleInstance.file.Write(writer);
                }

                logger.Log($"Grabbing CRC from temporary file `{tempPath}`");
                uint crc = await CRCGrabber.GetCRCFromFile(tempPath);

                if (!compress)
                {
                    logger.Log($"Saving to '{targetPath}'");
                    File.Copy(tempPath, targetPath, true);
                }
                else
                {
                    logger.Log($"Compressing bundle and saving to '{targetPath}' (this may take multiple minutes)");
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
                            compressedBundle.Pack(writer, AssetBundleCompressionType.LZMA);
                        }
                    }

                    compressedBundle.Close();
                }

                File.Delete(tempPath);

                return crc;
            }
        }
    }
}
