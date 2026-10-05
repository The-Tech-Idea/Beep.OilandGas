using System;

namespace Beep.OilandGas.Models.Core.Refusals
{
    /// <summary>
    /// The application refusing a request — bad input, a missing record, data that does not allow it — with a sentence
    /// written for the person who asked.
    /// </summary>
    /// <remarks>
    /// <para>
    /// OILGAS-CATCH-01. The API answered every <see cref="ArgumentException"/> and <see cref="InvalidOperationException"/>
    /// with its message as the refusal, and so did the controllers' own catches. Those are the framework's types too — EF,
    /// the collections, the JSON reader throw them — so a fault reached the caller as though it were a rule, in the
    /// framework's words. This type is thrown only by the application, and its <see cref="Sentence"/> is, by contract,
    /// words for the caller: the API answers it as problem details (RFC 9457 <c>detail</c>) and reports nothing, because a
    /// refusal is an answer, not a failure. Anything else is a failure: reported, answered 500 with its reference.
    /// </para>
    /// <para>
    /// The pattern is Microsoft's (eShop's domain exception, mapped once by the API) and ABP's <c>UserFriendlyException</c>.
    /// A domain's own exception family derives from this one when its messages are refusals of what the caller sent.
    /// </para>
    /// <para>
    /// <b>Never build the sentence from a caught exception's text</b> — that is a fault's words reaching the caller under a
    /// refusal's name. A caught failure is reported, and the refusal (if there is one) says what the person can act on.
    /// </para>
    /// </remarks>
    public class RefusalException : Exception
    {
        public RefusalException(RefusalKind kind, string sentence)
            : base(sentence)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sentence);
            Kind = kind;
            Sentence = sentence;
        }

        public RefusalException(RefusalKind kind, string sentence, Exception innerException)
            : base(sentence, innerException)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sentence);
            Kind = kind;
            Sentence = sentence;
        }

        /// <summary>What is refused, and so the status the API answers with.</summary>
        public RefusalKind Kind { get; }

        /// <summary>What the caller is told: the application's own words, written for them.</summary>
        public string Sentence { get; }

        /// <summary>What was sent cannot be done as sent.</summary>
        public static RefusalException Invalid(string sentence) => new(RefusalKind.Invalid, sentence);

        /// <summary>What was named does not exist.</summary>
        public static RefusalException NotFound(string sentence) => new(RefusalKind.NotFound, sentence);

        /// <summary>The data as it stands does not allow it.</summary>
        public static RefusalException Conflict(string sentence) => new(RefusalKind.Conflict, sentence);

        /// <summary>The caller may not do it.</summary>
        public static RefusalException Forbidden(string sentence) => new(RefusalKind.Forbidden, sentence);
    }
}
