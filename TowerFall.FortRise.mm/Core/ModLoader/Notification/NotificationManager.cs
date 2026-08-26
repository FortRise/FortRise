#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace FortRise;

public sealed class NotificationManager
{
    public const int MaxVisible = 4;
    public const float SlideTime = 0.25f;
    public const float Margin = 4f;
    public const float Padding = 4f;
    public const float Gap = 3f;
    public const float AccentWidth = 2f;
    public const float MaxTextWidth = 120f;
    public const float LineHeight = 10f;

    private readonly ConcurrentQueue<NotificationMessage> pending = new();
    private readonly List<NotificationMessage> messages = [];

    public IReadOnlyList<NotificationMessage> Messages => messages;

    public static NotificationManager Instance { get; private set; } = null!;

    public NotificationManager()
    {
        Instance = this;
    }

    public void Send(string message, NotificationType type, ModuleMetadata? sentBy, float duration = 5f)
    {
        pending.Enqueue(new NotificationMessage()
        {
            Message = message,
            Type = type,
            SentBy = sentBy,
            Duration = Math.Max(duration, 0f)
        });
    }

    public void Update(float delta)
    {
        SpriteFont? font = TFGame.Font;
        if (font is null)
        {
            return;
        }

        while (messages.Count < MaxVisible && pending.TryDequeue(out var queued))
        {
            Layout(queued, font);
            queued.Y = TargetY(messages.Count);
            messages.Add(queued);
        }

        for (int i = messages.Count - 1; i >= 0; i -= 1)
        {
            var message = messages[i];
            message.Elapsed += delta;

            float outStart = SlideTime + message.Duration;
            if (message.Elapsed >= outStart + SlideTime)
            {
                messages.RemoveAt(i);
                continue;
            }

            if (message.Elapsed < SlideTime)
            {
                message.Slide = Ease.BackOut(message.Elapsed / SlideTime);
            }
            else if (message.Elapsed < outStart)
            {
                message.Slide = 1f;
            }
            else
            {
                message.Slide = 1f - Ease.CubeIn((message.Elapsed - outStart) / SlideTime);
            }
        }

        float lerp = Math.Min(1f, delta * 12f);
        for (int i = 0; i < messages.Count; i += 1)
        {
            var message = messages[i];
            message.Y = MathHelper.Lerp(message.Y, TargetY(i), lerp);
        }
    }

    private float TargetY(int index)
    {
        float y = Margin;
        for (int i = 0; i < index; i += 1)
        {
            y += messages[i].Height + Gap;
        }
        return y;
    }

    private static void Layout(NotificationMessage message, SpriteFont font)
    {
        string header = message.SentBy is { } meta
            ? (string.IsNullOrEmpty(meta.DisplayName) ? meta.Name : meta.DisplayName)
            : "FortRise";
        message.Header = header.ToUpperInvariant();
        message.Lines = font.WrapText(message.Message.ToUpperInvariant(), MaxTextWidth);

        float textWidth = font.MeasureString(message.Header).X;
        foreach (var line in message.Lines)
        {
            textWidth = Math.Max(textWidth, font.MeasureString(line).X);
        }

        message.Width = AccentWidth + Padding * 2 + textWidth;
        message.Height = Padding * 2 + LineHeight * (message.Lines.Length + 1);
    }
}
