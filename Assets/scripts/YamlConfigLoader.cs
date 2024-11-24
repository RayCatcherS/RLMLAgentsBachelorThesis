using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

public static class YamlConfigLoader {
    public static int GetMaxSteps(string yamlFilePath, string behaviorName) {
        string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
        string filePath = Path.Combine(projectRootPath, yamlFilePath);

        if(File.Exists(filePath)) {
            string yamlContent = File.ReadAllText(filePath);

            // Deserializza il contenuto del file YAML
            var deserializer = new DeserializerBuilder().Build();
            var yamlData = deserializer.Deserialize<YamlConfig>(yamlContent);

            // Verifica se il comportamento specificato esiste
            if(yamlData.Behaviors != null && yamlData.Behaviors.ContainsKey(behaviorName)) {
                return yamlData.Behaviors[behaviorName].MaxSteps;
            } else {
                Debug.LogError($"Comportamento '{behaviorName}' non trovato nel file YAML.");
                return -1;
            }
        } else {
            Debug.LogError($"File YAML non trovato: {filePath}");
            return -1;
        }
    }


}

public class YamlConfig {
    [YamlMember(Alias = "behaviors")]
    public Dictionary<string, Behavior> Behaviors { get; set; }
}

public class Behavior {
    [YamlMember(Alias = "trainer_type")]
    public string TrainerType { get; set; }

    [YamlMember(Alias = "hyperparameters")]
    public Hyperparameters Hyperparameters { get; set; }

    [YamlMember(Alias = "network_settings")]
    public NetworkSettings NetworkSettings { get; set; }

    [YamlMember(Alias = "reward_signals")]
    public Dictionary<string, RewardSignal> RewardSignals { get; set; }

    [YamlMember(Alias = "keep_checkpoints")]
    public int KeepCheckpoints { get; set; }

    [YamlMember(Alias = "max_steps")]
    public int MaxSteps { get; set; }

    [YamlMember(Alias = "time_horizon")]
    public int TimeHorizon { get; set; }

    [YamlMember(Alias = "summary_freq")]
    public int SummaryFreq { get; set; }
}

public class Hyperparameters {
    [YamlMember(Alias = "batch_size")]
    public int BatchSize { get; set; }

    [YamlMember(Alias = "buffer_size")]
    public int BufferSize { get; set; }

    [YamlMember(Alias = "learning_rate")]
    public float LearningRate { get; set; }

    [YamlMember(Alias = "beta")]
    public float Beta { get; set; }

    [YamlMember(Alias = "epsilon")]
    public float Epsilon { get; set; }

    [YamlMember(Alias = "lambd")]
    public float Lambd { get; set; }

    [YamlMember(Alias = "num_epoch")]
    public int NumEpoch { get; set; }

    [YamlMember(Alias = "learning_rate_schedule")]
    public string LearningRateSchedule { get; set; }
}

public class NetworkSettings {
    [YamlMember(Alias = "normalize")]
    public bool Normalize { get; set; }

    [YamlMember(Alias = "hidden_units")]
    public int HiddenUnits { get; set; }

    [YamlMember(Alias = "num_layers")]
    public int NumLayers { get; set; }

    [YamlMember(Alias = "vis_encode_type")]
    public string VisEncodeType { get; set; }
}

public class RewardSignal {
    [YamlMember(Alias = "gamma")]
    public float Gamma { get; set; }

    [YamlMember(Alias = "strength")]
    public float Strength { get; set; }

    [YamlMember(Alias = "encoding_size")]
    public int EncodingSize { get; set; }

    [YamlMember(Alias = "learning_rate")]
    public float LearningRate { get; set; }
}

