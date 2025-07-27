using System;
using UnityEngine;

namespace TextureTools.Domain.Model
{
    [Serializable]
    public sealed class MaskSource
    {
        public Texture2D Source;
        public Channel SourceChannel = Channel.R;
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
