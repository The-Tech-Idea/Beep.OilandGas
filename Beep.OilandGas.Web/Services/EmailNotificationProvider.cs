using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Email notification channel for workflow events (SLA breaches, approvals, escalations).
/// Configured via appsettings.json: "Notifications:Email".
/// Part of Phase 5 experience & integration.
/// </summary>
public interface IEmailNotificationProvider
{
    /// <summary>Send an email notification.</summary>
    Task<bool> SendAsync(string toEmail, string subject, string body, bool isHtml = true);

    /// <summary>Send a workflow notification using a template.</summary>
    Task<bool> SendWorkflowNotificationAsync(string toEmail, string templateKey, Dictionary<string, string> parameters);
}

public class EmailNotificationProvider : IEmailNotificationProvider
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailNotificationProvider> _logger;

    private static readonly Dictionary<string, (string subject, string body)> Templates = new()
    {
        ["STEP_ASSIGNED"] = ("Task Assigned: {{StepName}}", "<p>A new task has been assigned to you: <strong>{{StepName}}</strong></p><p>Workflow: {{WorkflowName}}</p><p>Entity: {{EntityDescription}}</p><p>Due: {{DueDate}}</p><p><a href='{{Route}}'>View Task</a></p>"),
        ["APPROVAL_REQUESTED"] = ("Approval Required: {{StepName}}", "<p>Your approval is required for: <strong>{{StepName}}</strong></p><p>Workflow: {{WorkflowName}}</p><p>Requested by: {{RequestedBy}}</p><p>Amount: {{Amount}}</p><p><a href='{{Route}}'>Review & Approve</a></p>"),
        ["APPROVAL_GRANTED"] = ("Approved: {{StepName}}", "<p><strong>{{StepName}}</strong> has been approved by {{ApprovedBy}}.</p><p>Workflow: {{WorkflowName}}</p><p>Next step: {{NextStep}}</p>"),
        ["APPROVAL_REJECTED"] = ("Rejected: {{StepName}}", "<p><strong>{{StepName}}</strong> has been rejected by {{RejectedBy}}.</p><p>Reason: {{Reason}}</p><p><a href='{{Route}}'>View Details</a></p>"),
        ["SLA_BREACHED"] = ("URGENT: SLA Breached — {{StepName}}", "<p style='color:red'><strong>SLA Breach detected!</strong></p><p>Step: {{StepName}} exceeded its {{SlaHours}} hour SLA.</p><p>Elapsed: {{ElapsedHours}} hours.</p><p>Action taken: {{EscalationAction}}</p><p><a href='{{Route}}'>Take Action</a></p>"),
        ["SLA_AT_RISK"] = ("SLA At Risk — {{StepName}}", "<p>The following task is approaching its deadline:</p><p><strong>{{StepName}}</strong> — {{PercentElapsed}}% of SLA elapsed.</p><p>Due: {{DueDate}}</p>"),
        ["ROLE_ELEVATION_GRANTED"] = ("Temporary Role Elevation Granted", "<p>You have been granted temporary elevation to <strong>{{ElevatedRole}}</strong>.</p><p>Effective: {{EffectiveFrom}} to {{EffectiveTo}}</p><p>Reason: {{Reason}}</p><p>This elevation is time-bound and will auto-expire.</p>"),
        ["SOD_WAIVER_GRANTED"] = ("SoD Waiver Granted — {{RuleName}}", "<p>A Segregation of Duties waiver has been granted for:</p><p><strong>{{RuleName}}</strong></p><p>Compensating Control: {{ControlType}}</p><p>Expires: {{ExpiryDate}} (90 days max)</p>"),
    };

    public EmailNotificationProvider(IConfiguration config, ILogger<EmailNotificationProvider>? logger = null)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        try
        {
            var smtpSection = _config.GetSection("Notifications:Email:Smtp");
            var host = smtpSection.GetValue<string>("Host");
            var port = smtpSection.GetValue<int>("Port");
            var from = smtpSection.GetValue<string>("From") ?? "noreply@beep-oilgas.com";

            if (string.IsNullOrWhiteSpace(host))
            {
                _logger?.LogDebug("SMTP not configured — email notification skipped");
                return false;
            }

            using var client = new SmtpClient(host, port);
            client.EnableSsl = smtpSection.GetValue("EnableSsl", true);

            var username = smtpSection.GetValue<string>("Username");
            var password = smtpSection.GetValue<string>("Password");
            if (!string.IsNullOrWhiteSpace(username))
                client.Credentials = new NetworkCredential(username, password);

            var message = new MailMessage(from, toEmail, subject, body)
            {
                IsBodyHtml = isHtml,
            };

            await client.SendMailAsync(message);

            _logger?.LogDebug("Email sent to {To}: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to send email to {To}: {Subject}", toEmail, subject);
            return false;
        }
    }

    public async Task<bool> SendWorkflowNotificationAsync(
        string toEmail, string templateKey, Dictionary<string, string> parameters)
    {
        if (!Templates.TryGetValue(templateKey, out var template))
        {
            _logger?.LogWarning("Unknown email template: {TemplateKey}", templateKey);
            return false;
        }

        var subject = template.subject;
        var body = template.body;

        foreach (var (key, value) in parameters)
        {
            subject = subject.Replace($"{{{{{key}}}}}", value);
            body = body.Replace($"{{{{{key}}}}}", value);
        }

        return await SendAsync(toEmail, subject, body, true);
    }
}
