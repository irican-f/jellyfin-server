using MediaBrowser.Model.Plugins;

namespace MediaBrowser.Providers.Plugins.PlexBot.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        ApiUrl = "http://serv.maktep.fr:8888";
    }

    /// <summary>
    /// Gets or sets the url to PlexBot API.
    /// </summary>
    public string ApiUrl { get; set; }
}
