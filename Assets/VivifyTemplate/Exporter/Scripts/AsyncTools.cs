using System.Threading.Tasks;

namespace VivifyTemplate.Exporter.Scripts
{
    public static class AsyncTools
    {
        public static async Task AwaitNextFrame()
        {
            await Task.Delay(300); // this kinda SUCKS!
        }
    }
}