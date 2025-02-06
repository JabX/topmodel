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
