# Configuration <!-- {docsify-ignore-all} -->

Pour démarrer votre projet TopModel, vous devez d'abord écrire un fichier de configuration. Celui-ci contient notamment :

- Le nom de l'application
- Le répertoire racine des fichiers de modèle
- La configuration des modules de générateurs
- Un système de filtre (tags) pour la sélection des générateurs par langage sur lesquels TopModel sera utilisé.

Exemple :

```yaml
# topmodel.config
---
app: Hello World
```

Le fichier de configuration doit s'appeler `topmodel.config` ou `topmodel.[NOM DE L'APPLICATION].config`.

## Modules de générateurs

Chaque module de générateurs possède sa propre configuration. Néanmoins, la structure globale reste identique pour tous.

1. Commencer par le nom du module
2. Ajouter ensuite une liste de configurations
3. Chaque configuration **doit** préciser une liste de `tags`
4. Chaque configuration **doit** préciser le `outputDirectory`
5. Chaque configuration peut préciser des variables globales.
6. Chaque configuration peut préciser des variables par `tag`.
7. Chaque configuration peut préciser si elle doit ignorer les valeurs par défaut (`ignoreDefaultValues`).

Exemple :

```yaml
javascript:
  - tags:
      - Interne
      - Externe
      - Common
    variables:
      root: ../sources/front
    outputDirectory: "{root}/src"
    domainPath: common/domains
```

## Variables

Chaque générateur peut définir et utiliser des **variables** dans sa configuration. Une variable n'est donc scopée qu'au générateur qui la définit.

Les variables sont nécessairement des strings et ne peuvent donc être utilisées que dans des paramètres de type string. Il n'est pas exclu par la suite de gérer par la suite les nombres et les booléens, mais pour rester simple dans un premier temps ils ont été exclus.

Elles se définissent entre crochets (`{variable}`) et peuvent faire l'objet de transformations (`:upper`, `:pascal`, `:camel`...) de la même façon que les variables des templates dans les domaines et décorateurs (par exemple : `{root:lower}`).

Il existe **3 types de variables** :

### Variables globales

Les variables globales sont utilisables dans tous les paramètres (string) de toutes les générateurs, sans restriction. Elles seront remplacées à l'initialisation du générateur par la valeur qui a été définie dans la section `variables` de la configuration.

La variable globale `{app}` est définie par défaut avec la valeur de la propriété `app` de la configuration est peut donc être utilisée partout. Elle peut bien sûr être surchargée si besoin.

### Variables contextuelles

Il existe 2 variables "contextuelles", dont la valeur est automatiquement renseignée selon l'objet qui est généré, et qui sont utilisables dans certaines propriétés spécifiques des générateurs (selon leur implémentation). Ce sont :

- `{module}`, qui sera renseigné avec la valeur du module du fichier courant lors de la génération d'une classe ou d'un endpoint.

- `{lang}`, qui est utilisé lors de la génération de fichiers de ressources pour identifier la langue courante.

### Variables par tag

Un générateur peut choisir d'implémenter des variables qui ont des valeurs différentes **selon le tag** du fichier. De ce fait, un fichier ayant plusieurs tags résultant en des valeurs de paramètres différentes sera à priori généré plusieurs fois pour correspondre à chacune des valeurs possibles.

```yaml
javascript:
  - tags:
      - Interne
      - Externe
      - Common
    variables:
      root: ../sources/front
    tagVariables:
      Externe:
        tag: externe
      Common:
        tag: common
      Interne:
        tag: interne
    outputDirectory: "{root}/src"
    modelRootPath: "{tag}/model"
    resourceRootPath: "{tag}/locale"
    apiClientRootPath: "{tag}/services"
    fetchPath: "{tag}/server"
    domainPath: common/domains
```

`tagVariables` permet pour un tag de définir une liste de variables qui ne seront appliquées que lors de la génération de fichiers avec ce tag-là. Toutes les propriétés du générateur ne supportent pas les variables par tag.

**Une variable globale peut être surchargée par une variable par tag**, mais dans ce cas cette variable devient une variable par tag (et par conséquent n'est plus supportée dans les champs qui ne peuvent pas avoir de variables par tag). Une variable par tag existe dès lors qu'elle est définie par au moins un tag, et si un tag ne renseigne pas une variable par tag définie dans un autre tag, alors sa valeur pour ce tag sera automatiquement renseignée à `""` (ce qui effacera effectivement la variable lors de la résolution de valeur du paramètre).

### Résolution des variables

Les variables globales sont résolues en premier (une fois que celles qui devaient être transformées en variables par tag l'ont été), puis les variables par tag, et enfin les variables contextuelles. Cela implique en particulier que les variables contextuelles (`{module}` et `{lang}`) peuvent être "écrasées" par d'autres variables, et ainsi perdre leur contextualité. C'est rarement souhaitable, donc il vaut mieux en général éviter de provoquer des surcharges non intentionnelles.

`modgen` affichera un warning s'il trouve une variable non définie ou non supportée dans un paramètre (et précisera pourquoi, en particulier si la variable contextuelle ou par tag n'est pas supportée par le paramètre). Les variables non définies seront gardées telles quelles dans la génération, avec leurs `{}` (au cas où ça soit le comportement voulu dans ce cas précis, ce qui reste peu probable).

### Intersection de tags

Nous nous plaçons dans le cadre de la génération du tag `tag-a` d'une classe `A`, si cette classe a besoin d'importer une classe `B` qui n'est générée qu'avec le tag `tag-b` ce qui a un impact sur l'import à ajouter à A. Dans ce cas, le tag utilisé pour la résolution des variables par tag dans l'écriture de l'import de `B` dans `A` est `tag-b`.

## Autres propriétés de configuration globales

- **`modelRoot`**

  Permet de définir une autre racine pour le modèle que l'emplacement du fichier de configuration. A utiliser si vous voulez séparer l'emplacement du fichier de config du reste du modèle. (Attention : tous les chemins de fichiers dans les `uses` sont relatifs au `modelRoot`, et tous les fichiers `.tmd` doivent être dedans.)

- **`lockFileName`**

  Permet de surcharger le lockfile (`topmodel.lock` par défaut).

- **`ignoredFiles`**

  Permet de lister des chemins de fichiers générés qu'il ne faudra pas regénérer lors de générations successives. A utiliser avec parcimonie, pour contourner un manquement du générateur utilisé, en attendant un correctif du générateur ou de trouver une meilleure solution. Un fichier ignoré enregistrera un warning lors de la génération, et il est obligatoire de spécifier un commentaire pour chaque exclusion pour la justifier.

- **`noWarn`**

  Permet de désactiver les warnings listés. A utiliser avec parcimonie également, en général ces warnings ne sont pas là pour rien 😉.

- **`pluralizeTableNames`**

  Permet de renseigner le `sqlName` des classes par défaut avec leur `pluralName`, au lieu d'utiliser le `name`.
