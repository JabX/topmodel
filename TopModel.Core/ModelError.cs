﻿using System.Text;
using TopModel.Core.FileModel;

namespace TopModel.Core;

public class ModelError
{
    private readonly string _message;
    private readonly object _objet;
    private readonly Reference? _reference;

    internal ModelError(object objet, string message, Reference? reference = null)
    {
        _message = message;
        _objet = objet;
        _reference = reference;
    }

    public ModelFile File => _objet switch
    {
        ModelFile file => file,
        Class classe => classe.ModelFile,
        Endpoint endpoint => endpoint.ModelFile,
        IProperty { Class: Class classe } => classe.ModelFile,
        IProperty { Endpoint: Endpoint endpoint } => endpoint.ModelFile,
        Alias alias => alias.ModelFile,
        Domain domain => domain.ModelFile,
        _ => throw new ArgumentException("Type d'objet non supporté.")
    };

    public Class? Class => _objet switch
    {
        Class classe => classe,
        IProperty { Class: Class classe } => classe,
        _ => null
    };

    public Endpoint? Endpoint => _objet switch
    {
        Endpoint endpoint => endpoint,
        IProperty { Endpoint: Endpoint endpoint } => endpoint,
        _ => null
    };

    public Reference? Location => _reference ?? _objet.GetLocation();

    public IProperty? Property => _objet as IProperty;

    public string Message => string.Format(_message, _reference?.ReferenceName);

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(File.Path);
        sb.Append(Location?.Position ?? string.Empty);

        sb.Append(" - ");
        sb.Append(Message);
        sb.Append(" (");
        sb.Append(File);

        if (Class != null)
        {
            sb.Append($"/{Class.Name}");
        }
        else if (Endpoint != null)
        {
            sb.Append($"/{Endpoint.Name}");
        }
        else if (_objet is Alias)
        {
            sb.Append("/{alias}");
        }
        else if (_objet is Domain d)
        {
            sb.Append($"/{d.Name}");
        }

        switch (Property)
        {
            case RegularProperty rp:
                sb.Append($"/{rp.Name}");
                break;
            case AssociationProperty rp:
                sb.Append("/{association}");
                break;
            case AliasProperty rp:
                sb.Append("/{alias}");
                break;
            case CompositionProperty rp:
                sb.Append("/{composition}");
                break;
        }

        sb.Append(')');

        return sb.ToString();
    }
}
