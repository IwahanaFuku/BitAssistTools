namespace TextureTools.Shared
{
    public readonly struct Result<T>
    {
        public readonly bool IsOk;
        public readonly string Error;
        public readonly T Value;

        private Result(bool ok, T v, string e) { IsOk = ok; Value = v; Error = e; }
        public static Result<T> Ok(T v) => new Result<T>(true, v, null);
        public static Result<T> Fail(string e) => new Result<T>(false, default, e);
    }
}
