
namespace ContainerNinja.Contracts.Services
{
    public interface IAudioService
    {
        byte[] StripNoiseAndTrimSilence(byte[] data);
    }
}
