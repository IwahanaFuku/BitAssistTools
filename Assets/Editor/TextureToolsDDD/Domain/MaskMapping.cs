using System;
using UnityEngine;

namespace TextureTools.Domain.Model
{
    [Serializable]
    public sealed class MaskSource
    {
        public Texture2D Source;
        public Channel SourceChannel = Channel.R;
        public bool Invert = false;   // ← 追加：この入力チャンネルを反転するか
    }

    [Serializable]
    public sealed class MaskMapping
    {
        public MaskSource OutR;
        public MaskSource OutG;
        public MaskSource OutB;
        public MaskSource OutA;
    }
}
