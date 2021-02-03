using Hazel;

namespace Reactor.Example
{
    [RegisterCustomRpc]
    public class ExampleRpc : PlayerCustomRpc<ExamplePlugin, ExampleRpc.Data>
    {
        public ExampleRpc(ExamplePlugin plugin) : base(plugin)
        {
        }

        public readonly struct Data
        {
            public readonly string Message;

            public Data(string message)
            {
                Message = message;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

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
