﻿using Medior.Shared.Entities;

namespace Medior.Web.Server.Models;

public class DesktopHubSession
{
    public static DesktopHubSession Empty { get; } = new();

    public string ConnectionId { get; init; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
}
