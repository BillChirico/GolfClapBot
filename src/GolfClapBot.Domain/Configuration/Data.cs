namespace GolfClapBot.Domain.Configuration;

public class Data
{
    public required List<string>? TrainingData { get; set; }

    public required List<string>? RestrictedPhrases { get; set; }
}