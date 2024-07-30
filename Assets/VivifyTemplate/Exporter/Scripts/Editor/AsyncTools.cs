using System.Threading.Tasks;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public static class AsyncTools
    {
        public static async Task AwaitNextFrame()
        {
            await Task.Delay(300); // this kinda SUCKS!
        }
    }
}