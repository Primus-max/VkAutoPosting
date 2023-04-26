using System;

public class Message
{
    public long MessageId { get; set; }
    public string Text { get; set; }
    public string ImagePath { get; set; }

    public Message(long messageId, string text, string imagePath)
    {
        MessageId = messageId;
        Text = text;
        ImagePath = imagePath;
    }
}
