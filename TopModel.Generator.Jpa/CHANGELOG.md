## 1.5.5

- [06a1a](https://github.com/klee-contrib/topmodel/commit/06a1a02dbfb8361e918744f588d82de084805b13) [JPA] Erreur lorsqu'on génère une entité persistée alors que le type sql n'est pas défini
Fix [#446](https://github.com/klee-contrib/topmodel/issues/446)

## 1.5.4

Release technique

## 1.5.3

- [4add3](https://github.com/klee-contrib/topmodel/commit/4add3156138fae8612ff1f511a4c460d929c00ea) [JPA] Incompatibilité entre décorator et référence Fix [#441](https://github.com/klee-contrib/topmodel/issues/441)

> **Attention** : Nécessite modgen v2.4.2

## 1.5.2

- [af781](https://github.com/klee-contrib/topmodel/commit/af7812819384051ca347c2c207fdc4f26ea7fb5a) [JPA] annotation @Digits incomplète. Fix [#440](https://github.com/klee-contrib/topmodel/issues/440)

## 1.5.1

- [f54d39](https://github.com/klee-contrib/topmodel/commit/f54d3932d6cf316ed6d49b0f006fd4572bf826a8) [JPA] Fix annotation digits && size

## 1.5.0

- [e48e5a](https://github.com/klee-contrib/topmodel/commit/e48e5a0e1ec3ab5634cb0ac4d5af78c55c8bfae7) [JPA] Ajouter les annotations de validations @Size ou @Digits aux Dtos des propriétés dont les domains définissent Length ou Scale 
  Fix #437

> BREAKING CHANGE : Certaines api risquent de renvoyer des erreurs `400`

## 1.4.0

Compatibilité avec `ignoredFiles` de TopModel 2.4

## 1.3.0

- [#434](https://github.com/klee-contrib/topmodel/pull/434) - Enumérations représentant totalement l'entité. Ajout du mode `enumsAsEnums`

## 1.2.5

- [48855](https://github.com/klee-contrib/topmodel/commit/48855ffee1a89e8157770b720c3dbc72aa563241) [JPA] Fix api client génériques

## 1.2.4

- [afbcb](https://github.com/klee-contrib/topmodel/commit/afbcbd1801f334cf660dfceba430ec0090eb8bbd) [JPA] Amélioration typage méthode, annotation et paramètre Fix #429, #430

## 1.2.3

- [deeed](https://github.com/klee-contrib/topmodel/commit/deeed8d1703ee0009d642b681469f01a65abcabb) [JPA] annotation @NotNull non générée Fix #428


## 1.2.2

- [90a2c](https://github.com/klee-contrib/topmodel/commit/90a2c757fd7580dddd0bce938374db702db853a2) [JPA] Fix génération séquence

## 1.2.0

- [4e3b6b](https://github.com/klee-contrib/topmodel/commit/4e3b6b7072937a40aa0717b585c0bf231908d5e7) [JPA] Le spring client doit retourner un ResponseEntity (pour gérer les différents codes Http)

> BREAKING CHANGE: Les api clientes générées avec `SpringClient` ne renvoient plus l'objet `D` directement, mais un objet `ResponseEntity<D>`.

## 1.1.3

- [8e6c7e](https://github.com/klee-contrib/topmodel/commit/8e6c7e91211edf29108254fc0aef630157c69c90) [JPA] Fix annotation Column sur association manyToMany
- [a8cfea](https://github.com/klee-contrib/topmodel/commit/a8cfea155d4bcfe6d196cd9e8de2e529d16dea98) [JPA] Fix import alias de composition dans un module distant


## 1.1.2

- [429ffe](https://github.com/klee-contrib/topmodel/commit/429ffe4fc8c135fa4b13d36300b7875b7568206d) [JPA] Suppression de l'initialisation des newable types pour les dtos

## 1.1.1

- [95ebd](https://github.com/klee-contrib/topmodel/commit/95ebd7e79a6e482aaa6b7268fd6efe3084a26ff1) [JPA] Suite refacto

## 1.1.0

- [5849b8](https://github.com/klee-contrib/topmodel/commit/5849b8a30aa2a69954bc9fd2a6d2957ae1c52a82) [JPA] Créer un mode FeignClient dans le générateur d'API client
  Fix #419

- [db1f14](https://github.com/klee-contrib/topmodel/commit/db1f14fd5aa5e71f9667a448f419dfa5838b42dc) [JAVA] annotation absente sur un champ issu d'une composition #414

Breaking changes :
- Suppression du mode `enumShortcut`
- Les DAO des listes de références ne sont plus générés. La première génération risque de les supprimer
  - Annuler la suppression des DAO utilisés. Normalement, il y en a peu, d'où la suppression de la génération automatique...

## 1.0.11
- [d31beb](https://github.com/klee-contrib/topmodel/commit/d31beb5e0d42178e62f6b19316abcbbccde8884d) Fix Initialisation enum dans le cas d'alias ou d'association : cas null

## 1.0.10
- [acddcfe](https://github.com/klee-contrib/topmodel/commit/acddcfe1ed07577a7188768d674ee805764da6d4) Fix Initialisation enum dans le cas d'alias ou d'association

## 1.0.9

- [`e01da3f`](https://github.com/klee-contrib/topmodel/commit/e01da3f1d3b8c0dc39fe1eb8e206b953efb4b882) Problème import java entre deux classes générées Fix #398

## 1.0.8

- [`ab967cd`](https://github.com/klee-contrib/topmodel/commit/ab967cd621e914618d141d62d5182f113fbc306c) Correction converter dans le cas de composition

## 1.0.7

- [#395](https://github.com/klee-contrib/topmodel/pull/395) - Accolades sur le "if liste null".

## 1.0.6

- [`e0f01b8e`](https://github.com/klee-contrib/topmodel/commit/e0f01b8ea3d404aa196cfacd85f85564462bf581) Correction régression nullable

## 1.0.5

- [`97bc094a`](https://github.com/klee-contrib/topmodel/commit/97bc094a94e52167fd0bb86d1aca5308dbfc0593)
  - Enums :
    - Les setters ne sont plus générés
    - Les valeurs sont placés en premier
    - Ajout de l'annotation `@Transiant`
    - Les DAOS ne sont plus générés
    - Les `;` en fin d'enum ne sont plus générés lorsqu'ils sont inutiles
  - L'attribut `nullable` n'est plus renseigné lorsqu'il s'agit de la valeur par défaut

BREAKING CHANGES : - Les setters ne sont plus générés - les DAOS n'étant plus générés, ceux existant seront supprimés à la première génération

## 1.0.4

- [`aafe5e0c`](https://github.com/klee-contrib/topmodel/commit/aafe5e0c0b286a610e783d41d06da9ff74232c6a) - Fix formattage hashcode

## 1.0.3

Version initiale.
