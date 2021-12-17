using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Xabe.FFmpeg;

namespace FFmpegSample;

public class Program
{
    static Program()
    {
        FFmpeg.SetExecutablesPath(Path.Join(Environment.CurrentDirectory, "Libs", "FFmpeg"));
    }

    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException();
        }

        var filePath = args[0];
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException();
        }

        var tempFilePath = Path.GetTempFileName();
        tempFilePath = Path.ChangeExtension(tempFilePath, "wav");

        var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
        var audioStream = mediaInfo.AudioStreams.First()
            .SetChannels(1)
            .SetSampleRate(16000)
            .SetBitrate(16)
            .SetCodec(AudioCodec.pcm_s16le);

        _ = await FFmpeg.Conversions.New()
            .AddStream(audioStream)
            .SetOutput(tempFilePath)
            .SetOutputFormat(Format.wav)
            .SetOverwriteOutput(true)
            .SetOutputTime(TimeSpan.FromSeconds(30))  // Limit audio time 30s
            .AddParameter("-map_metadata -1", ParameterPosition.PostInput)  // Remove metadata
            .Start();

        try
        {
            var speechToken = Environment.GetEnvironmentVariable("SPEECH_TOKEN");
            var speechResion = Environment.GetEnvironmentVariable("SPEECH_RESION");
            var speechConfig = SpeechConfig.FromSubscription(speechToken, speechResion);
            var sourceLanguageConfig = SourceLanguageConfig.FromLanguage("ja-JP");
            using var audioConfig = AudioConfig.FromWavFileInput(tempFilePath);
            using var speechRecognizer = new SpeechRecognizer(speechConfig, sourceLanguageConfig, audioConfig);
            var result = await speechRecognizer.RecognizeOnceAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Console.WriteLine("exit");
    }
}