using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
namespace VivifyTemplate.Exporter.Scripts.Editor.ShaderKeywordRewriter
{
    internal static class AssetsToolsExtensions
    {
        /*internal static void PrintFieldNodes(this AssetsFile assetsFile, string fieldName)
        {
            TypeTreeType typeTreeType = assetsFile.Metadata.FindTypeTreeTypeByID((int)AssetClassID.Material);

            void PrintNode(int index)
            {
                TypeTreeNode node = typeTreeType.Nodes[index];
                Console.WriteLine($"AddNode(typeTreeType, {node.ByteSize}, {node.Level}, 0x{node.MetaFlags:X}, \"{node.GetNameString(typeTreeType.StringBuffer)}\", {node.RefTypeHash}, {string.Join(" | ", Enum.GetValues(typeof(TypeTreeNodeFlags)).Cast<TypeTreeNodeFlags>().Where(f => (node.TypeFlags.HasFlag(f) && f != 0) || (f == 0 && node.TypeFlags == 0)).Select(f => $"TypeTreeNodeFlags.{f}"))}, \"{node.GetTypeString(typeTreeType.StringBuffer)}\", {node.Version});");
            }

            int start = typeTreeType.Nodes.FindIndex(n => n.GetNameString(typeTreeType.StringBuffer) == fieldName);
            PrintNode(start);

            for (int i = start + 1; typeTreeType.Nodes[i].Level > typeTreeType.Nodes[start].Level; ++i)
            {
                PrintNode(i);
            }
        }*/

        internal static void InitializeField(this AssetTypeValueField valueField, TypeTreeType typeTreeType, string fieldName)
        {
            AssetTypeTemplateField templateField = new AssetTypeTemplateField();
            templateField.FromTypeTree(typeTreeType, fieldName);

            valueField.TemplateField.Children.Add(templateField);
            valueField.Children.Add(ValueBuilder.DefaultValueFieldFromTemplate(templateField));
        }

        internal static void AppendNode(this TypeTreeType typeTreeType, int byteSize, byte level, uint metaFlags, string nameStr, ulong refTypeHash, TypeTreeNodeFlags typeFlags, string typeStr, ushort version)
        {
            uint nameStrOffset = typeTreeType.GetStringOffset(nameStr);
            uint typeStrOffset = typeTreeType.GetStringOffset(typeStr);

            typeTreeType.Nodes.Add(new TypeTreeNode
            {
                ByteSize = byteSize,
                Index = (uint)typeTreeType.Nodes.Count,
                Level = level,
                MetaFlags = metaFlags,
                NameStrOffset = nameStrOffset,
                RefTypeHash = refTypeHash,
                TypeFlags = typeFlags,
                TypeStrOffset = typeStrOffset,
                Version = version,
            });
        }

        internal static void FromTypeTree(this AssetTypeTemplateField assetTypeTemplateField, TypeTreeType typeTreeType, string fieldName)
        {
            int startIndex = typeTreeType.Nodes.FindIndex(n => n.GetNameString(typeTreeType.StringBuffer) == fieldName);

            if (startIndex == -1)
            {
                throw new InvalidOperationException($"No field named '{fieldName}'");
            }

            int endIndex = typeTreeType.Nodes.FindIndex(startIndex + 1, n => n.Level == typeTreeType.Nodes[startIndex].Level);

            if (endIndex == -1)
            {
                endIndex = typeTreeType.Nodes.Count;
            }

            TypeTreeType dummy = new TypeTreeType()
            {
                Nodes = new List<TypeTreeNode>(typeTreeType.Nodes.Skip(startIndex).Take(endIndex - startIndex)),
                StringBufferBytes = typeTreeType.StringBufferBytes,
            };

            assetTypeTemplateField.FromTypeTree(dummy);
        }

        private static uint GetStringOffset(this TypeTreeType typeTreeType, string str)
        {
            int index = TypeTreeType.COMMON_STRING_TABLE.IndexOf(str + '\0', StringComparison.Ordinal);

            if (index != -1)
            {
                return (uint)index | 0x80000000;
            }

            index = typeTreeType.StringBuffer.IndexOf(str + '\0', StringComparison.Ordinal);

            if (index == -1)
            {
                index = typeTreeType.StringBuffer.Length;
                typeTreeType.StringBuffer += $"{str}\0";
            }

            return (uint)index;
        }
    }
}
