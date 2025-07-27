namespace BitAssistTools.Domain.Models.Renaming
{
    /// <summary>
    /// リネーム操作の種類を表現する列挙型
    /// </summary>
    public enum RenameOperationType
    {
        /// <summary>接頭辞・接尾辞の追加</summary>
        PrefixSuffix,
        
        /// <summary>文字列の置換</summary>
        StringReplace,
        
        /// <summary>テクスチャ名への変更</summary>
        TextureName,
        
        /// <summary>マテリアル名への変更</summary>
        MaterialName,
        
        /// <summary>番号の付与</summary>
        AddNumber,
        
        /// <summary>番号の除去</summary>
        RemoveNumber,
        
        /// <summary>Prefabアセットのリネーム</summary>
        PrefabAsset
    }
}