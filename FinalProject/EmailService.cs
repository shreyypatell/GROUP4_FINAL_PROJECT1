using System;
using System.Threading.Tasks;
using SmorcIRL.TempMail;
using SmorcIRL.TempMail.Models;

public class EmailService
{
    private MailClient _client;

    public EmailService()
    {
        _client = new MailClient();
    }

    public async Task<string> RegisterAndGetEmailAsync()
    {
        // Log in to mail client
       await _client.Login("shreyypatell@belgianairways.com", "86NBs67s2UAmmiu");

       return _client.Email;
    }

    public async Task<string> GenerateTemporaryEmailAndGetEmailAsync(string password)
    {  // Generate Temporary Email   
        MailClient client = new MailClient();
        await client.GenerateAccount(password);

        Console.WriteLine("New Generated email is: " + client.Email);
        
        return client.Email;
    }
    public async Task<string> GetLatestEmailAsync()
    {
        // Get all messages
        MessageInfo[] messages = await _client.GetAllMessages();

        string latestMessageId = messages[messages.Length].Id;

        // Get the message details
        MessageDetailInfo message = await _client.GetMessage(latestMessageId);

        return message.Subject;
    }
}
