using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfigController : Singleton<ConfigController>
{
    public static readonly string SERVER_HTTPS = "server.https";
    public static readonly string SERVER_URL = "server.url";
    public static readonly string SERVER_CLIENT_USER_NAME = "server.client-user-name";
    public static readonly string SERVER_CLIENT_SECRET = "server.client-secret";
    public static readonly string SERVER_OAUTH_LOGIN_URL = "server.oauth-login-url";
    public static readonly string SERVER_OAUTH_REFRESH_TOKEN_URL = "server.oauth-refresh-token-url";

    public static readonly string SSL_ENABLED = "ssl.enabled";
    public static readonly string SSL_KEYSTORE_PATH = "ssl.keystore.path";
    public static readonly string SSL_KEYSTORE_PASSWORD = "ssl.keystore.password";

    public static readonly string EVAL_CORRECT_DISTANCE = "evaluation.correct-distance.";
    public static readonly string EVAL_CORRECT_DISTANCE_ABS = "evaluation.correct-distance-absolute.";
    public static readonly string EVAL_CORRECT_ANGLE = "evaluation.correct-angle.";
    public static readonly string EVAL_CORRECT_ANGLE_ABS = "evaluation.correct-angle-absolute.";
    public static readonly string EVAL_COLOR_CONSUMPTION = "evaluation.color-consumption.";
    public static readonly string EVAL_COLOR_WASTAGE = "evaluation.color-wastage.";
    public static readonly string EVAL_COLOR_USAGE = "evaluation.color-usage.";
    public static readonly string EVAL_FULLY_PRESSED_TRIGGER = "evaluation.fully-pressed-trigger.";
    public static readonly string EVAL_AVERAGE_SPEED = "evaluation.average-speed.";
    public static readonly string EVAL_AVERAGE_COAT_THICKNESS = "evaluation.average-coat-thickness.";

    public static readonly string OPT_VALUE = "optimal-value";
    public static readonly string THRESHOLD1 = "threshold1";
    public static readonly string THRESHOLD2 = "threshold2";
    public static readonly string THRESHOLD3 = "threshold3";

    public static readonly string EVAL_CORRECT_DISTANCE_OPT_VALUE = EVAL_CORRECT_DISTANCE + OPT_VALUE;
    public static readonly string EVAL_CORRECT_DISTANCE_THRESHOLD1 = EVAL_CORRECT_DISTANCE + THRESHOLD1;
    public static readonly string EVAL_CORRECT_DISTANCE_THRESHOLD2 = EVAL_CORRECT_DISTANCE + THRESHOLD2;
    public static readonly string EVAL_CORRECT_DISTANCE_THRESHOLD3 = EVAL_CORRECT_DISTANCE + THRESHOLD3;
    public static readonly string EVAL_CORRECT_ANGLE_OPT_VALUE = EVAL_CORRECT_ANGLE + OPT_VALUE;
    public static readonly string EVAL_CORRECT_ANGLE_THRESHOLD1 = EVAL_CORRECT_ANGLE + THRESHOLD1;
    public static readonly string EVAL_CORRECT_ANGLE_THRESHOLD2 = EVAL_CORRECT_ANGLE + THRESHOLD2;
    public static readonly string EVAL_CORRECT_ANGLE_THRESHOLD3 = EVAL_CORRECT_ANGLE + THRESHOLD3;
    public static readonly string EVAL_COLOR_CONSUMPTION_OPT_VALUE = EVAL_COLOR_CONSUMPTION + OPT_VALUE;
    public static readonly string EVAL_COLOR_CONSUMPTION_THRESHOLD1 = EVAL_COLOR_CONSUMPTION + THRESHOLD1;
    public static readonly string EVAL_COLOR_CONSUMPTION_THRESHOLD2 = EVAL_COLOR_CONSUMPTION + THRESHOLD2;
    public static readonly string EVAL_COLOR_CONSUMPTION_THRESHOLD3 = EVAL_COLOR_CONSUMPTION + THRESHOLD3;
    public static readonly string EVAL_COLOR_WASTAGE_OPT_VALUE = EVAL_COLOR_WASTAGE + OPT_VALUE;
    public static readonly string EVAL_COLOR_WASTAGE_THRESHOLD1 = EVAL_COLOR_WASTAGE + THRESHOLD1;
    public static readonly string EVAL_COLOR_WASTAGE_THRESHOLD2 = EVAL_COLOR_WASTAGE + THRESHOLD2;
    public static readonly string EVAL_COLOR_WASTAGE_THRESHOLD3 = EVAL_COLOR_WASTAGE + THRESHOLD3;
    public static readonly string EVAL_COLOR_USAGE_OPT_VALUE = EVAL_COLOR_USAGE + OPT_VALUE;
    public static readonly string EVAL_COLOR_USAGE_THRESHOLD1 = EVAL_COLOR_USAGE + THRESHOLD1;
    public static readonly string EVAL_COLOR_USAGE_THRESHOLD2 = EVAL_COLOR_USAGE + THRESHOLD2;
    public static readonly string EVAL_COLOR_USAGE_THRESHOLD3 = EVAL_COLOR_USAGE + THRESHOLD3;
    public static readonly string EVAL_FULLY_PRESSED_TRIGGER_OPT_VALUE = EVAL_FULLY_PRESSED_TRIGGER + OPT_VALUE;
    public static readonly string EVAL_FULLY_PRESSED_TRIGGER_THRESHOLD1 = EVAL_FULLY_PRESSED_TRIGGER + THRESHOLD1;
    public static readonly string EVAL_FULLY_PRESSED_TRIGGER_THRESHOLD2 = EVAL_FULLY_PRESSED_TRIGGER + THRESHOLD2;
    public static readonly string EVAL_FULLY_PRESSED_TRIGGER_THRESHOLD3 = EVAL_FULLY_PRESSED_TRIGGER + THRESHOLD3;
    public static readonly string EVAL_AVERAGE_SPEED_OPT_VALUE = EVAL_AVERAGE_SPEED + OPT_VALUE;
    public static readonly string EVAL_AVERAGE_SPEED_THRESHOLD1 = EVAL_AVERAGE_SPEED + THRESHOLD1;
    public static readonly string EVAL_AVERAGE_SPEED_THRESHOLD2 = EVAL_AVERAGE_SPEED + THRESHOLD2;
    public static readonly string EVAL_AVERAGE_SPEED_THRESHOLD3 = EVAL_AVERAGE_SPEED + THRESHOLD3;
    public static readonly string EVAL_AVERAGE_COAT_THICKNESS_OPT_VALUE = EVAL_AVERAGE_COAT_THICKNESS + OPT_VALUE;
    public static readonly string EVAL_AVERAGE_COAT_THICKNESS_THRESHOLD1 = EVAL_AVERAGE_COAT_THICKNESS + THRESHOLD1;
    public static readonly string EVAL_AVERAGE_COAT_THICKNESS_THRESHOLD2 = EVAL_AVERAGE_COAT_THICKNESS + THRESHOLD2;
    public static readonly string EVAL_AVERAGE_COAT_THICKNESS_THRESHOLD3 = EVAL_AVERAGE_COAT_THICKNESS + THRESHOLD3;
    public static readonly string EVAL_CORRECT_DISTANCE_ABS_OPT_VALUE = EVAL_CORRECT_DISTANCE_ABS + OPT_VALUE;
    public static readonly string EVAL_CORRECT_DISTANCE_ABS_THRESHOLD1 = EVAL_CORRECT_DISTANCE_ABS + THRESHOLD1;
    public static readonly string EVAL_CORRECT_DISTANCE_ABS_THRESHOLD2 = EVAL_CORRECT_DISTANCE_ABS + THRESHOLD2;
    public static readonly string EVAL_CORRECT_DISTANCE_ABS_THRESHOLD3 = EVAL_CORRECT_DISTANCE_ABS + THRESHOLD3;
    public static readonly string EVAL_CORRECT_ANGLE_ABS_OPT_VALUE = EVAL_CORRECT_ANGLE_ABS + OPT_VALUE;
    public static readonly string EVAL_CORRECT_ANGLE_ABS_THRESHOLD1 = EVAL_CORRECT_ANGLE_ABS + THRESHOLD1;
    public static readonly string EVAL_CORRECT_ANGLE_ABS_THRESHOLD2 = EVAL_CORRECT_ANGLE_ABS + THRESHOLD2;
    public static readonly string EVAL_CORRECT_ANGLE_ABS_THRESHOLD3 = EVAL_CORRECT_ANGLE_ABS + THRESHOLD3;

    public static readonly string WINDOWED_MODE = "system.windowed-mode";
    public static readonly string MAX_FILE_SIZE = "system.max-file-size";
    public static readonly string LANGUAGE = "system.language";


    private readonly Dictionary<string, float> defaultFloatValues = new Dictionary<string, float>
    {
        { EVAL_CORRECT_DISTANCE_OPT_VALUE, 100 },
        { EVAL_CORRECT_DISTANCE_THRESHOLD1, 10 },
        { EVAL_CORRECT_DISTANCE_THRESHOLD2, 10 },
        { EVAL_CORRECT_DISTANCE_THRESHOLD3, 10 },
        { EVAL_CORRECT_ANGLE_OPT_VALUE, 100 },
        { EVAL_CORRECT_ANGLE_THRESHOLD1, 20 },
        { EVAL_CORRECT_ANGLE_THRESHOLD2, 20 },
        { EVAL_CORRECT_ANGLE_THRESHOLD3, 20 },
        { EVAL_COLOR_CONSUMPTION_OPT_VALUE, 300 },
        { EVAL_COLOR_CONSUMPTION_THRESHOLD1, 10 },
        { EVAL_COLOR_CONSUMPTION_THRESHOLD2, 10 },
        { EVAL_COLOR_CONSUMPTION_THRESHOLD3, 10 },
        { EVAL_COLOR_WASTAGE_OPT_VALUE, 30 },
        { EVAL_COLOR_WASTAGE_THRESHOLD1, 0 },
        { EVAL_COLOR_WASTAGE_THRESHOLD2, 30 },
        { EVAL_COLOR_WASTAGE_THRESHOLD3, 30 },
        { EVAL_COLOR_USAGE_OPT_VALUE, 70 },
        { EVAL_COLOR_USAGE_THRESHOLD1, 10 },
        { EVAL_COLOR_USAGE_THRESHOLD2, 10 },
        { EVAL_COLOR_USAGE_THRESHOLD3, 10 },
        { EVAL_FULLY_PRESSED_TRIGGER_OPT_VALUE, 100 },
        { EVAL_FULLY_PRESSED_TRIGGER_THRESHOLD1, 5 },
        { EVAL_FULLY_PRESSED_TRIGGER_THRESHOLD2, 5 },
        { EVAL_FULLY_PRESSED_TRIGGER_THRESHOLD3, 5 },
        { EVAL_AVERAGE_SPEED_OPT_VALUE, 0.4f },
        { EVAL_AVERAGE_SPEED_THRESHOLD1, 20 },
        { EVAL_AVERAGE_SPEED_THRESHOLD2, 20 },
        { EVAL_AVERAGE_SPEED_THRESHOLD3, 20 },
        { EVAL_AVERAGE_COAT_THICKNESS_OPT_VALUE, 90 },
        { EVAL_AVERAGE_COAT_THICKNESS_THRESHOLD1, 20 },
        { EVAL_AVERAGE_COAT_THICKNESS_THRESHOLD2, 10 },
        { EVAL_AVERAGE_COAT_THICKNESS_THRESHOLD3, 10 },
        { EVAL_CORRECT_DISTANCE_ABS_OPT_VALUE, 17.5f },
        { EVAL_CORRECT_DISTANCE_ABS_THRESHOLD1, 14.28f },
        { EVAL_CORRECT_DISTANCE_ABS_THRESHOLD2, 10 },
        { EVAL_CORRECT_DISTANCE_ABS_THRESHOLD3, 10 },
        { EVAL_CORRECT_ANGLE_ABS_OPT_VALUE, 90 },
        { EVAL_CORRECT_ANGLE_ABS_THRESHOLD1, 10 },
        { EVAL_CORRECT_ANGLE_ABS_THRESHOLD2, 10 },
        { EVAL_CORRECT_ANGLE_ABS_THRESHOLD3, 10 },
        { MAX_FILE_SIZE, 1000 }
    };

    private Dictionary<string, bool> defaultBoolValues = new Dictionary<string, bool>
    {
        { SERVER_HTTPS, false }, { SSL_ENABLED, false }, { WINDOWED_MODE, false }
    };

    private Dictionary<string, string> defaultStringValues = new Dictionary<string, string>
    {
        { SERVER_URL, "localhost:8080" },
        { SERVER_CLIENT_USER_NAME, "handlevrclient" },
        { SERVER_CLIENT_SECRET, "XY7kmzoNzl100" },
        { SERVER_OAUTH_LOGIN_URL, "localhost:8080/oauth/token?grant_type=password" },
        { SERVER_OAUTH_REFRESH_TOKEN_URL, "localhost:8080/oauth/token?grant_type=refresh_token" },
        { SSL_KEYSTORE_PATH, "/StreamingAssets/SSL/handlevr.p12" },
        { SSL_KEYSTORE_PASSWORD, "passwort" },
        { LANGUAGE, "de_DE" }
    };

    private Dictionary<string, string> properties;

    void Awake()
    {
        ReadConfigFile();
        Screen.fullScreen = !GetBoolValue(WINDOWED_MODE);
    }

    public float GetFloatValue(string propertyName)
    {
        TryGetValue(propertyName, out float value);
        return value;
    }

    public bool GetBoolValue(string propertyName)
    {
        TryGetValue(propertyName, out bool value);
        return value;
    }

    public string GetStringValue(string propertyName)
    {
        TryGetValue(propertyName, out string value);
        return value;
    }

    public bool TryGetValue(string propertyName, out float value)
    {
        if (!defaultFloatValues.TryGetValue(propertyName, out value))
            return false;
        if (!properties.ContainsKey(propertyName))
            return false;
        string stringValue = properties[propertyName];
        return float.TryParse(stringValue, out value);
    }

    public bool TryGetValue(string propertyName, out bool value)
    {
        if (!defaultBoolValues.TryGetValue(propertyName, out value))
            return false;
        if (!properties.ContainsKey(propertyName))
            return false;
        string stringValue = properties[propertyName];
        return bool.TryParse(stringValue, out value);
    }

    public bool TryGetValue(string propertyName, out string value)
    {
        if (!defaultStringValues.TryGetValue(propertyName, out value))
            return false;
        if (!properties.ContainsKey(propertyName))
            return false;
        value = properties[propertyName];
        return true;
    }

    private void ReadConfigFile()
    {
        properties = new Dictionary<string, string>();
        if (!File.Exists(DataController.Instance.configFilePath))
            return;
        StreamReader file = new StreamReader(DataController.Instance.configFilePath);
        string line;
        while ((line = file.ReadLine()) != null)
        {
            if (line.StartsWith("#") || !line.Contains("="))
                continue;
            line = line.Trim();
            int indexSign = line.IndexOf('=');
            properties.Add(line.Substring(0, indexSign), line.Substring(indexSign + 1, line.Length - indexSign - 1));
        }

        if (!Path.IsPathRooted(GetStringValue(SSL_KEYSTORE_PATH)))
            properties[SSL_KEYSTORE_PATH] =
                Path.Combine(DataController.Instance.streamingAssetsPath, GetStringValue(SSL_KEYSTORE_PATH));
    }

    public string GetURLPrefix()
    {
        return GetBoolValue(SERVER_HTTPS) ? "https://" : "http://";
    }

    public string GetFullServerURL()
    {
        return GetURLPrefix() + GetStringValue(SERVER_URL);
    }

    public int GetMaxFileSize()
    {
        int maxFileSize = (int) GetFloatValue(MAX_FILE_SIZE);
        return Math.Min(maxFileSize, 2000);
    }

    public string GetLanguage()
    {
        string language = GetStringValue(LANGUAGE);
        return language == "en_US" ? language : "de_DE";
    }
}