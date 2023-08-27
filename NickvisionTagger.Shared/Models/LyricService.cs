using System;
using System.Net.Http;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// A service for looking up and downloading lyrics for a music file
/// </summary>
public class LyricService : IDisposable
{
    private bool _disposed;
    private readonly HttpClient _http;
    
    /// <summary>
    /// Constructs a LyricService
    /// </summary>
    public LyricService()
    {
        _disposed = false;
        _http = new HttpClient();
    }
    
    /// <summary>
    /// Finalizes the LyricService
    /// </summary>
    ~LyricService() => Dispose(false);
    
    /// <summary>
    /// Frees resources used by the LyricService object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the LyricService object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        _http.Dispose();
        _disposed = true;
    }
}