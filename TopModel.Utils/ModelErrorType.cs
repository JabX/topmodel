namespace TopModel.Utils;

public enum ModelErrorType
{
    /// <summary>
    /// Code d'erreur par défaut
    /// </summary>
    TMD0000,

    /// <summary>
    /// La classe doit avoir au moins une propriété non composée et au plus une clé primaire pour être définie comme `reference`.
    /// </summary>
    TMD0001,

    /// <summary>
    /// L'import '{use.ReferenceName}' ne doit être spécifié qu'une seule fois
    /// </summary>
    TMD0002,

    /// <summary>
    /// Le nom '{endpoint.Name}' est déjà utilisé.
    /// </summary>
    TMD0003,

    /// <summary>
    /// La propriété '{propertyReference.Name}' est déjà référencée dans la définition de l'alias.
    /// </summary>
    TMD0004,

    /// <summary>
    /// La classe '{classe}' est définie plusieurs fois dans le fichier ou une de ses dépendences.
    /// </summary>
    TMD0005,

    /// <summary>
    /// La classe '{classe}' doit avoir au moins une propriété non composée, au plus une clé primaire et au moins une `value` pour être définie comme `enum`.
    /// </summary>
    TMD0006,

    /// <summary>
    /// Le domaine '{domain}' est déjà défini.
    /// </summary>
    TMD0007,

    /// <summary>
    /// Le flux de données '{dataFlow}' est défini plusieurs fois dans le fichier ou une de ses dépendences.
    /// </summary>
    TMD0008,

    /// <summary>
    /// La classe '{0}' doit avoir une (et une seule) clé primaire pour être référencée dans une association.
    /// </summary>
    TMD1001,

    /// <summary>
    /// La classe est introuvable dans le fichier
    /// </summary>
    TMD1002,

    /// <summary>
    /// Le fichier est introuvable dans les dépendances du fichier
    /// </summary>
    TMD1003,

    /// <summary>
    /// La propriété '{{0}}' est introuvable sur la classe '{aliasedClass}'
    /// </summary>
    TMD1004,

    /// <summary>
    /// Le domaine '{0}' est introuvable.
    /// </summary>
    TMD1005,

    /// <summary>
    /// L'endpoint est introuvable dans le fichier
    /// </summary>
    TMD1006,

    /// <summary>
    /// Le fichier référencé '{use.ReferenceName}' est introuvable
    /// </summary>
    TMD1007,

    /// <summary>
    /// Le décorateur est introuvable dans le fichier.
    /// </summary>
    TMD1008,

    /// <summary>
    /// Le décorateur '{decoratorRef.ReferenceName}' est déjà présent dans la liste des décorateurs de la classe '{classe}'.
    /// </summary>
    TMD1009,

    /// <summary>
    /// Impossible d'appliquer le décorateur '{decoratorRef.ReferenceName}' à la classe '{classe}' : seul un 'extends' peut être spécifié.
    /// </summary>
    TMD1010,

    /// <summary>
    /// La propriété '{ukPropRef.ReferenceName}' n'existe pas sur la classe '{classe}'.
    /// </summary>
    TMD1011,

    /// <summary>
    /// La valeur '{valueRef.Key.ReferenceName}' n'initialise pas les propriétés obligatoires suivantes.
    /// </summary>
    TMD1012,

    /// <summary>
    /// La valeur viole la contrainte d'unicité [{string.Join(", ", uk.Select(u => u.Name))}]
    /// </summary>
    TMD1013,

    /// <summary>
    /// La propriété '{mappedProperty.Name}' ne peut pas être mappée à '{currentProperty.Name}' car elle n'a pas le même domaine.
    /// </summary>
    TMD1014,

    /// <summary>
    /// La propriété '{mapping.Key.ReferenceName}' est déjà initialisée dans ce mapper.
    /// </summary>
    TMD1015,

    /// <summary>
    /// Plusieurs propriétés de la classe peuvent être mappées sur '{mapping.Value.Name}'.
    /// </summary>
    TMD1016,

    /// <summary>
    /// La propriété '{mappedProperty.Name}' ne peut pas être mappée à la composition '{currentProperty.Name}' car ce n'est pas une association.
    /// </summary>
    TMD1017,

    /// <summary>
    /// L'association '{mappedProperty.Name}' ne peut pas être mappée à la composition '{currentProperty.Name}' car l'association et la composition doivent toutes les deux être simples.
    /// </summary>
    TMD1018,

    /// <summary>
    /// La propriété '{mappedProperty.Name}' ne peut pas être mappée à la composition '{currentProperty.Name}' car elle n'a pas le même domaine que la clé primaire de la classe '{cp.Composition.Name}' composée.
    /// </summary>
    TMD1019,

    /// <summary>
    /// La classe '{mappedClass.Name}' ne peut pas être mappée sur la propriété '{currentProperty.Name}' car ce n'est pas une composition de cette classe.
    /// </summary>
    TMD1020,

    /// <summary>
    /// Le préfixe d'endpoint '{file.Options.Endpoints.Prefix}' doit être identique à celui de tous les fichiers de même nom et de même module.
    /// </summary>
    TMD1021,

    /// <summary>
    /// La définition de la conversion entre {df.Name} et {dt.Name} est déjà définie dans un autre converter.
    /// </summary>
    TMD1022,

    /// <summary>
    /// Le domaine '{prop.Domain}' doit définir un domaine de liste pour définir un alias liste sur la propriété '{prop.OriginalProperty}' de la classe '{prop.OriginalProperty?.Class}'.
    /// </summary>
    TMD1023,

    /// <summary>
    /// La propriété '{property}' ne peut pas être la cible d'un mapping car elle a été marquée comme 'readonly'.
    /// </summary>
    TMD1024,

    /// <summary>
    /// Aucun mapping n'a été trouvé sur ce mapper.
    /// </summary>
    TMD1025,

    /// <summary>
    /// Impossible de définir un 'extends' sur la classe '{classe}' abstraite.
    /// </summary>
    TMD1026,

    /// <summary>
    /// Le endpoint '{endpoint.Name}' définit un paramètre '{routeParamName}' dans sa route qui n'existe pas dans la liste des paramètres.
    /// </summary>
    TMD1027,

    /// <summary>
    /// Cette association ne peut pas avoir le type {ap.Type} car le domain {ap.Class.PrimaryKey.Single().Domain} ne contient pas de définition de AsDomain
    /// </summary>
    TMD1028,

    /// <summary>
    /// Cette association sur la classe '{classe}' doit définir un rôle.
    /// </summary>
    TMD1029,

    /// <summary>
    /// Impossible de définir un 'extends' sur la classe '{classe}' abstraite.
    /// </summary>
    TMD1030,

    /// <summary>
    /// La classe '{classe}' et sa classe parente '{classe.Extends}' doivent toutes les deux être des `enum`.
    /// </summary>
    TMD1031,

    /// <summary>
    /// La propriété '{mapping.Property.Name}' doit être une composition de la même classe que '{mapping.TargetProperty.Name}' pour définir un mapping entre les deux.
    /// </summary>
    TMD1032,

    /// <summary>
    /// La propriété '{mapping.Property.Name}' ne peut pas être une composition pour définir un mapping vers '{mapping.TargetProperty.Name}'.
    /// </summary>
    TMD1033,

    /// <summary>
    /// Le paramètre '{param.GetName()}' du mapper ne peut pas être obligatoire si l'un des paramètres précédents ne l'est pas.
    /// </summary>
    TMD1034,

    /// <summary>
    /// Le flux de données est introuvable dans le fichier ou l'une de ses références.
    /// </summary>
    TMD2000,

    /// <summary>
    /// Le fichier '{relativePath}' ne sera pas regénéré pour le motif : '{motif}'
    /// </summary>
    TMD8000,

    /// <summary>
    /// Le fichier '{ignoredFile}' dans `ignoredFiles` est introuvable.
    /// </summary>
    TMD8001,

    /// <summary>
    /// L'import {} n'est pas utilisé.
    /// </summary>
    TMD9001,

    /// <summary>
    /// Le trigram '{classe.Trigram}' est déjà utilisé.
    /// </summary>
    TMD9002,

    /// <summary>
    /// Le paramètre de requête '{queryParam.GetParamName()}' doit suivre tous les paramètres de route ou de body dans un endpoint.
    /// </summary>
    TMD9003,

    /// <summary>
    /// Le domaine '{domain.Name}' n'est pas utilisé.
    /// </summary>
    TMD9004,

    /// <summary>
    /// Le décorateur '{decorateur.Name}' n'est pas utilisé.
    /// </summary>
    TMD9005
}
