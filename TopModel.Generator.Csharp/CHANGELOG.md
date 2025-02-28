## 1.4.0

- [`50c3f6b`](https://github.com/klee-contrib/topmodel/commit/50c3f6b86a71da1e76ca1a63d4372120c788732e) - [C#] Mise au propre de la génération des accesseurs de liste de références

  **breaking changes** :

  - Les interfaces d'accesseurs de liste de références sont désormais séparés en 2, entre ceux qui sont sur des classes persistés et ceux sur des classes non-persistés. Cela permet d'arrêter d'avoir à implémenter un `partial` sur l'implémentation générée (qui n'a elle pas changé) avec les autres listes de références, vous pouvez donc simplement implémenter la deuxième interface directement, ou vous voulez.
  - Les interfaces et implémentations persistées sont générées avec un nom préfixé par `Db` (en plus du `referenceAccessorName`).
  - `referenceAccessorsInterfacePath` n'a plus de valeur par défaut et doit être renseigné pour que les accesseurs soit générés (puisqu'on ne veut plus du tout la générer à côté de l'implémentation 😉)
  - (Les commentaires générés dans les accesseurs sont désormais en français, comme le reste).

- [`b8595e4`](https://github.com/klee-contrib/topmodel/commit/b8595e4bc945c6668374985cdb4f4ace16a16623) - [C#] Plus d'options à "true" par défaut

  **breaking changes** : `enumForStaticReferences`, `usePrimaryConstructors` et `useRecords` sont désormais à `true` par défaut.

## 1.3.0

- [`cf05c8c`](https://github.com/klee-contrib/topmodel/commit/cf05c8c31d8f80179741b2c5d6a07888528207f7) - [C# ApiServer] Annotations [Required] sur les paramètres required
- [`865020e`](https://github.com/klee-contrib/topmodel/commit/865020e969ec65535f0aeaca9c7da09b61321710) - [C#] Fix détermination valueType pour enums avec genericType

**breaking change** : Le `required` est désormais correctement pris en compte sur les paramètres d'endpoint, ce qui va en particulier ajouter des annotations `[Required]` sur vos query params obligatoires. Vous devriez donc vérifier que la valeur de `required` dans le modèle correspond bien à la réalité de votre endpoint, ou alors vous pouvez simplement ajouter des `required: false` jusqu'à ce que le code généré ne change pas (mais ce n'est évidemment pas la meilleure solution 😉)

## 1.2.0

Compatibilité avec `ignoredFiles` de TopModel 2.4

## 1.1.2

- [`75ba587`](https://github.com/klee-contrib/topmodel/commit/75ba58725fcf8c3e0abb495bf60cc0d2c68ca3fa) - [C#] Ajout génération usings de converters dans les mappers

## 1.1.1

- [`c1ec016`](https://github.com/klee-contrib/topmodel/commit/c1ec01639dccc17ece05136ffe85ce1618d925fb) - [C# Server API] Fix ? et = null en trop pour bodyparam: true

## 1.1.0

- [`6aeba30`](https://github.com/klee-contrib/topmodel/commit/6aeba30068b86500e9d73b5d474f354e1e384979) - [C# Server API] Paramètres multipart toujours nullables (comme query)

  C'est un **petit breaking change** parce que tous les paramètres multipart (à priori les fichiers à upload, typés `IFormFile`) sont désormais générés nullables avec un `= null` derrière, commes les query params, ce qui nécessite de les mettre en dernier dans la liste des paramètres. La prochaine version de TopModel incluera une mise à jour du warning existant pour prendre en compte ce cas.

## 1.0.4

- [`2f1fe4a`](https://github.com/klee-contrib/topmodel/commit/2f1fe4a6b7d369b45c2b159c9e9f6b323eb225ff) - [C#] Fix using en trop si `requiredNonNullable`

## 1.0.3

Version initiale.
