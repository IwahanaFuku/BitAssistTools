namespace TextureTools.Domain.Model
{
    public readonly struct NormalStrength
    {
        public readonly float Value;
        public NormalStrength(float value) { Value = value; }
        public override string ToString() => $"NormalStrength({Value})";
    }
}
