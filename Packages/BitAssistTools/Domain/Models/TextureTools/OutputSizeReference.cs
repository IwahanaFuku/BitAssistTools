namespace BitAssistTools.Domain.Models.TextureTools
{
    /// <summary>
    /// 出力サイズの参照先
    /// </summary>
    public enum OutputSizeReference
    {
        /// <summary>
        /// 元画像のサイズを参照
        /// </summary>
        Raw = 0,
        
        /// <summary>
        /// Unityでリサイズされたサイズを参照
        /// </summary>
        Resize = 1
    }
}