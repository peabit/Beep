using NAudio.Wave;
using Quartz;

namespace Beep;

public class NotifyJob : IJob
{
    private readonly ILogger<NotifyJob> _logger;

    public NotifyJob(ILogger<NotifyJob> logger)
        => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Beep! {now}", DateTime.Now);

        await using var audioFile = new AudioFileReader("sound.wav");
        using var outputDevice = new WaveOutEvent();
        
        outputDevice.Init(audioFile);
        outputDevice.Play();

        await Task.Delay(audioFile.TotalTime);
    }
}