namespace CloudScribe.Notes.API.Configuration;

/// <summary>
/// Configuration settings for the database migration job.
/// </summary>
public class MigrationJobSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the migration job is enabled.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the application should stop if migrations fail.
    /// Default is true for safety.
    /// </summary>
    public bool StopApplicationOnFailure { get; set; } = true;
}
