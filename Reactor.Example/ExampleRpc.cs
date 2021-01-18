using Hazel;

namespace Reactor.Example
{
    [RegisterCustomRpc]
    public class ExampleRpc : CustomRpc<ExamplePlugin, PlayerControl, ExampleRpc.Data>
    {
        public ExampleRpc(ExamplePlugin plugin) : base(plugin)
        {
        }

        public readonly struct Data
        {
            public string Message { get; }

            public Data(string message)
            {
                Message = message;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Message);
        }

        public override Data Read(MessageReader reader)
        {
            return new Data(reader.ReadString());
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            Plugin.Log.LogWarning($"{innerNetObject.Data.PlayerId} sent \"{data.Message}\"");
        }
    }
}
