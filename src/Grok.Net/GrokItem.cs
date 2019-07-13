namespace GrokNet
{
    public sealed class GrokItem
    {
        public GrokItem(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public object Value { get; }
    }
}
