using System;
using System.Collections.Generic;
using System.Text;

namespace AnalyzersPracticeLibrary.Helpers;

public static class DiagnosticDescriptorBuilder
{
    public static IIdStep Create()
    {
        return new Builder();
    }

    public interface IIdStep
    {
        ITitleStep WithId(string id);
    }

    public interface ITitleStep
    {
        IMessageStep WithTitle(string title);
    }

    public interface IMessageStep
    {
        ICategoryStep WithMessage(string message);
    }

    public interface ICategoryStep
    {
        ISeverityStep WithCategory(string category);
    }

    public interface ISeverityStep
    {
        IEnabledStep WithSeverity(DiagnosticSeverity severity);
    }

    public interface IEnabledStep
    {
        IBuildStep EnabledByDefault();

        IBuildStep DisabledByDefault();
    }

    public interface IBuildStep
    {
        DiagnosticDescriptor Build();
    }

    private sealed class Builder :
        IIdStep,
        ITitleStep,
        IMessageStep,
        ICategoryStep,
        ISeverityStep,
        IEnabledStep,
        IBuildStep
    {
        private string _id = "";
        private string _title = "";
        private string _message = "";
        private string _category = "";
        private DiagnosticSeverity _severity;
        private bool _enabledByDefault;

        public ITitleStep WithId(string id)
        {
            _id = id;
            return this;
        }

        public IMessageStep WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public ICategoryStep WithMessage(string message)
        {
            _message = message;
            return this;
        }

        public ISeverityStep WithCategory(string category)
        {
            _category = category;
            return this;
        }

        public IEnabledStep WithSeverity(
            DiagnosticSeverity severity)
        {
            _severity = severity;
            return this;
        }

        public IBuildStep EnabledByDefault()
        {
            _enabledByDefault = true;
            return this;
        }

        public IBuildStep DisabledByDefault()
        {
            _enabledByDefault = false;
            return this;
        }

        public DiagnosticDescriptor Build()
        {
            return new DiagnosticDescriptor(
                id: _id,
                title: _title,
                messageFormat: _message,
                category: _category,
                defaultSeverity: _severity,
                isEnabledByDefault: _enabledByDefault);
        }

    }
}