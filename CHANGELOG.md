# TopModel.Generator (`modgen`)

Changelogs des modules :

- [`csharp`](./TopModel.Generator.Csharp/CHANGELOG.md)
- [`javascript`](./TopModel.Generator.Javascript/CHANGELOG.md)
- [`jpa`](./TopModel.Generator.Jpa/CHANGELOG.md)
- [`sql`](./TopModel.Generator.Sql/CHANGELOG.md)
- [`translation`](./TopModel.Generator.Translation/CHANGELOG.md)

## 2.4.3

- [`c2d41a4`](https://github.com/klee-contrib/topmodel/commit/c2d41a4a9c816244a5177ec4bb8fec895663b202) - [Core] Fix génération annotations non parsées Fix #443

## 2.4.2

- [`1522a6`](https://github.com/klee-contrib/topmodel/commit/df49cc8ff4ec6d5a3588664c3bb8048e8e7f16e5) - [Core] Fonctionnalité de debrayage ne fonctionne pas Fix [#442](https://github.com/klee-contrib/topmodel/issues/442)

## 2.4.1

- [`d762e5`](https://github.com/klee-contrib/topmodel/commit/df49cc8ff4ec6d5a3588664c3bb8048e8e7f16e5) - [Core] Correction logs en erreur à cause du markup

## 2.4.0

- [`#435`](https://github.com/klee-contrib/topmodel/pull/435) / [`#436`](https://github.com/klee-contrib/topmodel/pull/436) - `ignoredFiles`

  Il est désormais possible de "débrancher" la génération de certains fichiers par TopModel après leur première génération, en renseignant la propriété `ignoredFiles` dans la configuration avec le chemin vers les fichiers générés à ignorer. Cette fonctionnalité est **à utiliser avec parcimonie**, pour contourner un manquement du générateur utilisé, **en attendant** un correctif du générateur ou de trouver une meilleure solution. Un fichier ignoré enregistrera un warning lors de la génération, et il est obligatoire de spécifier un commentaire pour chaque exclusion pour la justifier.

  Cette évolution a été rendue possible par des évolutions structurantes sur la génération de fichiers au sein des générateurs TopModel. Par conséquent, **cette fonctionnalité ne sera disponible qu'avec les dernières versions des générateurs** (à ce jour). Si vous utilisez un générateur personnalisé, il devra également être mis à jour pour bénéficier de la fonctionnalité (et peut être d'autres nouvelles fonctionnalités dans le futur). En revanche, et comme promis, TopModel 2.4 reste 100% rétro-compatible avec des générateurs existants, donc si vous avez bloqué une version de générateur pour n'importe quelle raison, vous pouvez quand même faire cette mise à jour.

- [`df49cc8`](https://github.com/klee-contrib/topmodel/commit/df49cc8ff4ec6d5a3588664c3bb8048e8e7f16e5) - topmodel.lock à la racine de la config au lieu de la racine du modèle

  Il s'agit théoriquement d'un breaking change, mais il n'affectera que les utilisateurs qui ont spécifié un `modelRoot` explicite différent de `""` ou `"/"` dans leur configuration, et cela va simplement déplacer le lockfile. Il est toujours possible de le renommer.

## 2.3.4

- [`60efcc60efcc60fe4bb8e9f743e5577f5b0f0848392594'](https://github.com/klee-contrib/topmodel/commit/60efcc60fe4bb8e9f743e5577f5b0f0848392594) [Core] Correction recopie `domainParameters`

## 2.3.3

- [`75ba587`](https://github.com/klee-contrib/topmodel/commit/75ba58725fcf8c3e0abb495bf60cc0d2c68ca3fa) - Support pour évolution du générateur C#.

## 2.3.2

- [`f647621`](https://github.com/klee-contrib/topmodel/commit/f64762148497a86e171fd641bdfb8bc7f2ca237d) - [Core] preservePropertyCasing sans impact sur SqlName

## 2.3.1

- [`2d4e6e0`](https://github.com/klee-contrib/topmodel/commit/2d4e6e0198245ef34a5f3287ea0357d3979f7544) - [Core] Fix calcul 'Association.NameCamel'

## 2.3.0

- [`d51c93a`](https://github.com/klee-contrib/topmodel/commit/d51c93a867dbcb5bf9b9d79c816f0ad2bfc0b3a6) - `className` sur association (fix [#374](https://github.com/klee-contrib/topmodel/issues/374))
- [`5107614`](https://github.com/klee-contrib/topmodel/commit/5107614fb6c33b0c2155effa02c75fdb0efd77a3) - [Core] Warning ordre des paramètres d'endpoint qui inclus aussi les multiparts
- [`4037be2`](https://github.com/klee-contrib/topmodel/commit/4037be206bd5304cc62e64530205e96b3dc03b8e) - [Core/JS] Fix "strictIfUppercase" manquant sur camel/pascal case des noms de propriétés

  C'est peut être un petit breaking change parce que les noms de propriétés entièrement en majusucules sont désormais bien convertis dans la casse du langage cible (PascalCase / camelCase). Si vous avez besoin de conserver la vraie casse dans le fichier de modèle, vous pouvez toujours utiliser `preservePropertyCasing` sur la classe.

## 2.2.0

- [`992239c`](https://github.com/klee-contrib/topmodel/commit/992239c91f90e058e0ea45615e61a34b575100ad) - net6.0/8.0 => net8.0/9.0

  **breaking change** : TopModel ne supporte plus .NET 6, puisque son support est terminé. En revanche, on peut l'utiliser avec .NET 9 maintenan.

- [#412](https://github.com/klee-contrib/topmodel/pull/412) - [Generator] Vide les buffers lors du build des générateurs custom

## 2.1.3

- [`#403`](https://github.com/klee-contrib/topmodel/pull/403) - [Core] Autoriser types integer, boolean et number dans customProperties
- [`#405`](https://github.com/klee-contrib/topmodel/pull/405) - [Core] Ajout support des mappings dans les associations

## 2.1.2

- [`a59d49aa`](https://github.com/klee-contrib/topmodel/commit/a59d49aa9fab7eb072799daa74f3d9c34971378c) - [modgen] Fix retour du bug du hash sur les dossiers de modules vide

## 2.1.1

- [`a27223ef`](https://github.com/klee-contrib/topmodel/commit/a27223ef2969e3df59a455d1c37a86b43c0d834d) - [modgen] maxdepth = 3 lors de la recherche d'un fichier de config

  Au lieu de chercher à profondeur "infinie" les fichiers de config topmodel dans le répertoire courant, on s'arrête aux 3 premiers niveaux (pour éviter que ça soit très long...)

- [`67f27db3`](https://github.com/klee-contrib/topmodel/commit/67f27db3f5fe92b8bc3e8a49634134bbcf70f5ca) - [modgen] Build des générateurs custom si sources modifiées (avec hash dans le lockfile)
- [`460b5a19`](https://github.com/klee-contrib/topmodel/commit/460b5a198789299e0e2ad7da3a33ad4307de7f24) - [modgen] Sauvegarde des derniers fichiers builds en custom dans .modgen

  `modgen` va build vos générateurs customs au lancement, si les sources de la dernière version utilisée (hash sauvegardé dans le lockfile) ou buildée (hash sauvegardé dans `.modgen`) ont changé.

## 2.1.0

- [`#390`](https://github.com/klee-contrib/topmodel/pull/390) - `customProperties` sur classes, endpoints et propriétés
- [`4116b9`](https://github.com/klee-contrib/topmodel/commit/4116b9d0944d81b73d0f231c4101aa00cb11b850) - [modgen] Vérification des dernières versions de modgen + des générateurs au lancement
- [`6ed0a1`](https://github.com/klee-contrib/topmodel/commit/6ed0a14bd643a10f5668648db65cfa7a727b724d) - modgen -s qui modgen quand même

## 2.0.4

- [`9727a4`](https://github.com/klee-contrib/topmodel/commit/9727a440cbfab34c9a51702069402f51f72d56ad) - [modgen] Correction régression 2.0.3 si les répertoires des modules n'existent pas

## 2.0.3

- [`90ab7ff`](https://github.com/klee-contrib/topmodel/commit/90ab7ff1638da994b59964f8a432c3080428d17b) - [modgen] Ajout hash des modules dans le lockfile

  Cette évolution permet de tracer le contenu exact d'un module installé via son hash, pour détecter des cas de corruption et forcer une réinstallation si nécessaire. Puisque le hash n'existait pas avant, le premier lancement de cette version va forcer la réinstallation des modules pour le calculer (et le sauvegarder dans le lockfile).

## 2.0.1 - 2.0.2

Correctifs suite à la version majeure pour faire fonctionner l'installation du module `csharp` et des modules personnalisés.

## 2.0.0

- [`#383`](https://github.com/klee-contrib/topmodel/pull/383) - modgen 2.0

  Cette version majeure dissocie les générateurs de `modgen` lui-même, qui ira les installer à la demande selon la configuration. Pour plus d'informations, vous pouvez consulter l'[issue originale](https://github.com/klee-contrib/topmodel/issues/253) et la doc à jour, en particulier la page [Génération](https://klee-contrib.github.io/topmodel/#/generator).

  Les différents générateurs sont tous initialisés avec la version `1.0.3`, et ils auront désormais leurs changelogs respectifs.

  Ce changelog sera donc le dernier qui listera les évolutions des générateurs. Tous les évolutions présentées ici seront donc dans les versions `1.0.3` respectives de chaque générateur.

  Le seul """breaking change""" associé à cette montée de version est le fait qu'il vous faudra ajouter le répertoire `.modgen` à votre `.gitignore`. De plus, vous verrez que votre schéma JSON pour la configuration TopModel sera modifié pour n'inclure que les schémas des modules de générateurs que vous utilisez.

- [`#386`](https://github.com/klee-contrib/topmodel/pull/386) - Générateur SQL procédural pour Oracle + nouvelles options de générations
- [`8a7b4f5`](https://github.com/klee-contrib/topmodel/commit/8a7b4f5c02c43279fbc004e685becb8bb063fd12) - [JPA] Correction indentation hasshCode
- [`0c13a00`](https://github.com/klee-contrib/topmodel/commit/0c13a006b7f176a70df195b781bbbf01eb3f3dc4) - [JS] Fix code généré pour correspondre à prettier

  On va dire qu'on va justifier celui-là avec le fait qu'on release une 2.0 parce que cette modification va impacter **presque tout le code JS généré**, essentiellement pour remplacer des virgules par des points-virgules dans les définitions de type Typescript...

## 1.52.2

- HotFix sur [`#382`](https://github.com/klee-contrib/topmodel/issue/382)

## 1.52.1

- [`ac556c1`](https://github.com/klee-contrib/topmodel/commit/ac556c17dfc84ec52035ceddcbda0d956cf6b4a9) - [JS] Traductions de compositions générées qu'avec `extendedCompositions`
- [`#381`](https://github.com/klee-contrib/topmodel/issue/381) - [JAVA] SonarQube règles java:S2057 and java:S2162
- [`#382`](https://github.com/klee-contrib/topmodel/issue/382) - [JPA] Comparaison des clés primaires composites sur des associations (manyToMany explicite)

## 1.52.0

- [`#380`](https://github.com/klee-contrib/topmodel/pull/380) - Suppression de IFieldProperty (et donc alias de compositions)
- [`#379`](https://github.com/klee-contrib/topmodel/pull/379) - [C# + SQL] Compositions sur classes persistées (en JSON)
- [`ea971f22`](https://github.com/klee-contrib/topmodel/commit/ea971f22044c8831e161322454b8fd511aa866b4) - [JPA] Implémentation des compositions sur les classes persistées
- [`999b8e60`](https://github.com/klee-contrib/topmodel/commit/999b8e607017e6a7bf8c80c7a629f3a493569d33) - [JS] `extendedCompositions`

  L'ensemble de ces fonctionnalités permet **d'unifier les propriétés de composition avec les autres types de propriétés**, pour arrêter de les considérer différemment dans TopModel. Cela implique que vous pouvez désormais utiliser des compositions à tous les endroits où vous pouviez auparavant ne pas en mettre, en particulier :

  - Vous pouvez désormais créer des **alias sur des compositions**
  - Vous pouvez désormais **persister une composition en base de données** (dans une colonne JSON)
  - Vous pouvez désormais mapper des compositions (de la même classe) dans les mappers
  - Les compositions peuvent donc avoir un libellé et un trigramme.

  **breaking changes** :

  - Les alias sans `include` explicite incluent désormais les compositions. Si vous en avez, il faudra donc soit retirer la composition que vous avez manuellement recopiée (ce qui est le cas le plus probable), soit ajouter la composition dans l'`exclude`
  - Les mappers généreront des mappings automatiques entre compositions de même nom, classe et domaine (à voir si vous voulez le garder ou le retirer manuellement).
  - Si vous développez des générateurs personnalisés, la suppression du type `IFieldProperty` (pour représenter une propriété qui n'est pas une composition) vous impactera probablement dans ce que vous faites.

  **impacts génération** :

  - Les libellés de compositions sont désormais générés dans tous les fichiers de traductions (si vous les voulez dans les définitions d'entité JS, il vous faudra utiliser la nouvelle option `extendedCompositions` et une librairie à jour pour les interpréter).
  - [JS] L'ordre de `defaultValue` et `isRequired` a été inversé dans la génération.
  - [C#] Les annotations `[Required]` sont désormais générées sur les compositions `required` (les compositions étant `required` par défaut, cela devrait donc concerner la grande majorité de vos compositions).

- [`d2dab384`](https://github.com/klee-contrib/topmodel/commit/d2dab3849c42312a9ff01a6a1d19e58aed0803d4) - [JS] Fix clés de traduction générées à tort

  **breaking changes (JS)** :

  Si vous utilisiez des sous-modules, des clés de traductions en doublon étaient générées pour tous les alias, même si le libellé était inchangé. Ce n'est désormais plus le cas. La clé de traduction renseignée sur la propriété dans l'entité générée a toujours été celle du libellé original, donc si vous référenciez bien `Entity.property.label` dans votre code cela n'aura pas d'impact.

## 1.51.2

- [`06a32515`](https://github.com/klee-contrib/topmodel/commit/06a32515b689e371a222a01291df83eec7d389b5) - [C#] `noColumnOnAlias`, `useEFMigrations`, et `useLowerCaseSqlNames` à `true` par défaut

  C'est un **breaking change sur la config**, mais ces trois options sont déjà activées explicitement (et nécessaires) sur la plupart des projets...

- [`2d9847b0`](https://github.com/klee-contrib/topmodel/commit/2d9847b016082298acf43dad47cbeda80c06b236) - [C#] Retrait annotations `[NotMapped]` inutiles

  On ne génère plus ces annotations sur les compositions dans les classes non persistées, ce qui n'a jamais eu beaucoup de sens. Elles avaient été initialement mises pour EF Core lorsque l'on voulait faire des requêtes SQL manuelles directement sur un DTO qui avait une composition (car il plantait car la propriété n'était pas renseignée dans le SQL), mais cette limitation a depuis été levée. **Le code généré est donc différent**, mais cela ne devrait avoir **aucun impact** dans votre application.

- [`b37b78f6`](https://github.com/klee-contrib/topmodel/commit/b37b78f6c5c50db1e4c2382645852ee55b6bf7cc) - [C#] Simplification code généré pour client d'API

  Cela aurait dû être fait avec la 1.49.2 mais le travail avait été fait à moitié... **Le code généré est différent**, mais cela ne devrait avoir **aucun impact** dans votre application.

- [`364defa2`](https://github.com/klee-contrib/topmodel/commit/364defa27982638e03aa4d63e56c27e074e7258f) - [C#] Fix FK en trop sur DbContext

## 1.51.1

- [`c1be8736`](https://github.com/klee-contrib/topmodel/commit/c1be8736e67001a6c6745b6cf568623d42a7c999) - [C#] Fix verbatim manquant

## 1.51.0

- [`941b82f7`](https://github.com/klee-contrib/topmodel/commit/941b82f761dc4787c8627bc0f37eae7d6055702b) - [C#] Propriétés obligatoires manquantes en paramètres dans les mappers `to` vers une nouvelle instance.

  C'est un **tout petit breaking change** malgré le fait que les nouveaux paramètres sont facultatifs (sans `requiredNonNullable`, avec le mapper n'était pas généré avant), car il peut y avoir ambiguïté à l'appel avec l'autre mapper `to`. Le cas pourrait se produire avec un appel du style `dto.ToDb(new() { AutreId = autreId })`, que vous pouvez de toute façon probablement écrire `dto.ToDb(autreId)` maintenant 🙂.

- [`45fd16ff`](https://github.com/klee-contrib/topmodel/commit/45fd16ff0d7704cff9c1fefe35f5ff8fc2ad50b9) - [C#] Mapping enum <> pas enum avec Parse/GetName.

  Le cas était auparavant généré sans conversion et le code C# était donc en erreur.

- [`f07aa133`](https://github.com/klee-contrib/topmodel/commit/f07aa1336810e4622554457b64e9268477b78c9e) - [C#] Fix/Gestion génération associations/compositions si classe pas générée.

  S'il manquait la classe d'une composition ou d'une association (avec enum) d'une classe générée dans un générateur (à cause de son tag), on générait quand même la propriété avec un type inexistant (avec encore du code en erreur donc). Le code généré omet donc maintenant les compositions dans ce cas (de la même façon qu'il omettait déjà les mappers vers des classes inexistantes), et met le type non enum pour les associations. Normalement, le seul cas restant où il reste possible de générer du code invalide pour tag manquant sur des classes est dans les propriétés d'endpoints, mais en même temps ici il n'y a pas vraiment d'autre solution que d'inclure la classe manquante 😉.

- [`78a70093`](https://github.com/klee-contrib/topmodel/commit/78a700938cda924907ac1b2186ccea53ec26ffef) - [JPA] Fix TrimSlashes manquant sur EnumsPath + warning à tort sur les variables par tag
- [`dbfc6fb8`](https://github.com/klee-contrib/topmodel/commit/dbfc6fb82c183fe32face1e0c6378261bf25b3a7) - [JPA] Correction indentation spring client API

## 1.50.0

- [`7371cac8`](https://github.com/klee-contrib/topmodel/commit/7371cac83fda315aa4fa76965c254b9033ca709b) - [C#] `persistentReferencesModelPath` => `referencesModelPath`

  **(petit) breaking change** :

  `persistentReferencesModelPath` a été remplacé par `referencesModelPath`, qui concerne donc maintenant toutes les listes de références. Par conséquent, vos listes de références non persistées seront désormais générées au même endroit que vos listes de références persistées si vous utilisez ce paramètre. Cela ne devrait pas poser de problèmes particuliers (hormis peut être quelques usings à changer dans votre code applicatif), puisque l'objectif initial de ce paramètre était de pouvoir générer les listes de références persistées avec le modèle non persisté.

- [`524c41d9`](https://github.com/klee-contrib/topmodel/commit/524c41d9eab115b99d706136687fa87e6623a89d) - [C#DbContext] Fix `NoPersistence` non respecté.

## 1.49.3

- [`eb6f4ef`](https://github.com/klee-contrib/topmodel/commit/eb6f4ef7a6d703ddd34ae80bcfc0a7f07abce8e3) [JPA] Fix multiple :
  - Erreur de compilation dans le cas d'alias vers des associations
  - Correction javadoc lorsqu'une classe non accessible est utilisée

## 1.49.2

- [`e92be4d4`](https://github.com/klee-contrib/topmodel/commit/e92be4d4b9da18140daa80147cbf0ef8801fd4c4) - [C#Client] Gestion propre returns required/pas required

  **(petit) breaking change** :

  Le méthode `Deserialize<T>` n'est plus générée et son contenu est désormais inline dans le retour de la méthode.

  De plus, le check `if 204 return null` n'est désormais fait que si le `returns` n'est pas `required` (pour rappel, les compositions sont `required` par défaut, mais pas les autres propriétés). Si le check a été retiré alors qu'il était utile, alors vous devez passer `required: false` dans votre `returns`.

- [`23e6f4be`](https://github.com/klee-contrib/topmodel/commit/23e6f4be0913d02f0f78de2a1ef2bfde5ba945a8) - [C#Class] Pas de `[Required]` si `required`
- [`166c3fbb`](https://github.com/klee-contrib/topmodel/commit/166c3fbb16b1c2de1ff6d38baedc04afe3dca683) - [C#ClassGen] Fix usings en trop

## 1.49.1

- [#368](https://github.com/klee-contrib/topmodel/pull/368) - [JPA] Clef primaire composite : annotations @Id et @Convert non compatibles
- [#366](https://github.com/klee-contrib/topmodel/pull/366) - [SQL] Ordre de génération des séquences

## 1.49.0

- [#365](https://github.com/klee-contrib/topmodel/pull/365) - [C#] - `requiredNonNullable`

  Cette release ajoute une option de génération pour le générateur C#. Il n'y a pas d'impact sur les autres générateurs.

  **Impacts génération** :

  - Les mappers `to` sont désormais générés avec 2 surcharges, une avec l'instance cible et une autre sans, au lieu de n'en générer qu'une seule qui gère les deux cas.

  **(tout petits) breaking changes** :

  - Les implémentations de converter ne doivent plus inclure de `?` pour les types valeurs (il sera rajouté automatiquement par le générateur si besoin).
  - L'option `nonNullableTypes` a été divisée en `valueTypes` (pour renseigner les types valeurs qu'il faudra wrapper dans un `Nullable` avec un `?`) et en `nullableEnable` pour activer l'option de même nom (à priori, personne n'utilisait cette option jusqu'à présent, car il manquait justement `requiredNonNullable` pour qu'elle soit vraiment utile 😉).

## 1.48.2

- [`f3bb29`](https://github.com/klee-contrib/topmodel/commit/f3bb29cccebb7cf9680a014bb6d75e5f7d4d9150) - [JPA] Ajouter le constructeur sans argument dans le cas où la classe a une classe parente

## 1.48.1

- [`1e4600`](https://github.com/klee-contrib/topmodel/commit/1e460044b5fd9fc734c8be90bdc0fecd3bf2f97a) - [JPA] Ajouter le constructeur sans argument dans le cas où la classe a une classe parente

## 1.48.0

- [`78cd90`](https://github.com/klee-contrib/topmodel/commit/78cd90abbac03bd3359271034aa45d251803b2a2) - [JPA] Donner la possibilité de masquer l'annotation @Generated

- [`3293bd8`](https://github.com/klee-contrib/topmodel/commit/3293bd8f17e2e75f58bbfa703403137c4ac00223) - [JPA] Correction DAO correction visibilité interface

- [`#197`](https://github.com/klee-contrib/topmodel/issues/197)[JPA] Supprimer la règle qui empêche de faire des associations dans des classes non persistées

- [`e8fd3`](https://github.com/klee-contrib/topmodel/commit/e8fd3bc3282b0012d86ebbb46dc966ee65a3e3fb) - [JPA] Ne générer le constructeur par défaut que si un autre constructeur est généré. Y ajouter un commentaire

- [`#359`](https://github.com/klee-contrib/topmodel/issues/359) - [SQL] Mode "séquence", ajouter l'information "owned by"

## 1.47.3

- [`161a4`](https://github.com/klee-contrib/topmodel/commit/9c55eba0bb81f8042c05f28958a29fe6f94f7884) - [JPA] Fix génération constructeur avec property

## 1.47.2

- [`e8745`](https://github.com/klee-contrib/topmodel/commit/9c55eba0bb81f8042c05f28958a29fe6f94f7884) - [JPA] Correction import mapper from dans le cas d'une propriété

## 1.47.1

- [`9c55e`](https://github.com/klee-contrib/topmodel/commit/9c55eba0bb81f8042c05f28958a29fe6f94f7884) - [JS] Doner la possibilité de générer le client Angular en remplaçant les observables par des promesses hot fix

## 1.47.0

- [`4f39e`](https://github.com/klee-contrib/topmodel/commit/944fd8200a1fe05dfa9f6a1b2d23f12f102f64c1) - [JS] Doner la possibilité de générer le client Angular en remplaçant les observables par des promesses

## 1.46.6

- [`944fd`](https://github.com/klee-contrib/topmodel/commit/944fd8200a1fe05dfa9f6a1b2d23f12f102f64c1) - [JPA] Les imports d'un domaine ne sont pas reportés dans les repositories - Hot fix

## 1.46.5

- [`c301c5`](https://github.com/klee-contrib/topmodel/commit/c301c5d078914a20500e4f609929443fa99b0987) - [JPA] Fix #358 Les imports d'un domaine ne sont pas reportés dans les repositories

## 1.46.4

- [`f5a109`](https://github.com/klee-contrib/topmodel/commit/f5a109ca66dcb9105f8f977679f5b2cc8457cf55) - [C#] Fix ligne en trop mapper si required.

## 1.46.3

- [`b4d495`](https://github.com/klee-contrib/topmodel/commit/b4d49559404b93502f9b4f8bfa2aee114486f98d) [SQL] Traduction des libellés : augmentation taille champ & renommage de la pk. Suppression de la colonne locale si pas de langue par défaut

## 1.46.2

- [`#357`](https://github.com/klee-contrib/topmodel/issues/357) [SQL] Traduction des libellés en bdd avec double quotes au lieu de simple quotes
  Fix: #357

## 1.46.1

- [`92da932854689`](https://github.com/klee-contrib/topmodel/commit/92da932854689da9bcb83081a9ffcb3d7d08a0ed) [SQL] Suppression de la FK vers la locale car pg n'autorise pas de fk vers une colonne qui ne contient pas de clé d'unicité

## 1.46.0

- [`#354`](https://github.com/klee-contrib/topmodel/issues/354) [JS] Template pour les noms des fichiers contenant les api clientes

  **Breaking changes** :

  - Pour les utilisateurs angular, les services d'api client sont déplacés dans un fichier `*.service.ts`. Pour retrouver le comportement initial, définir la propriété `apiClientFilePath` à `{module}/{fileName}`

- [`a6c10f6`](https://github.com/klee-contrib/topmodel/commit/a6c10f6b4a44cc8c318c037bdfb5d38cb2b54d98) - Parallélisation de la génération de fichiers

  Avec cette release, `modgen` devrait être beaucoup plus rapide qu'avant pour générer les fichiers !

- [`785bb3a`](https://github.com/klee-contrib/topmodel/commit/785bb3a003dee49db3e610ad4ab72db0b0b4ca0d) - [C#ApiServer] Fix [FromQuery] manquant pour les arrays et [FromForm] en trop pour IFormFile

## 1.45.2

- [`dd31cf`](https://github.com/klee-contrib/topmodel/commit/dd31cf8282fdf0343e8337bbfe594da2309a9b76) - [JSAPIClient] Fix génération multipart + query param + fix spacing dans le code généré (Angular + Nuxt)

## 1.45.1

- [`a43cd0`](https://github.com/klee-contrib/topmodel/commit/a43cd0123e938d5c382ef04ca54ca7ebd04e144a) [JS] Fix enregistrement générateur ressources

## 1.45.0

- [`de48408`](https://github.com/klee-contrib/topmodel/commit/de48408da4232d0096da9e43e20055704bbebe20) [JPA] Implémenter les mappers de propriétés dans les mapers from

- [`d45ab79`](https://github.com/klee-contrib/topmodel/commit/d45ab794e98affa685935ebc1b1869c9e4b8b381) [Propriétés sur mappers] Fix affichage erreurs sur alias + fix alias mal recalculés en watch

- [`2ce3101`](https://github.com/klee-contrib/topmodel/commit/2ce31010a7e61b21f2ad6051b291b8d5770b27dc) [Core] Property param dans les mappers : l'alias "full" remonte est toujours en erreur

- [`abbb542`](https://github.com/klee-contrib/topmodel/commit/abbb5422de17bf8151117359c6f3baa00865914c) [Core] Utilisation du genericType sur alias "as" dans le cas général (fix #346)

- [#350](https://github.com/klee-contrib/topmodel/pull/350) Exposer paramName dans les templates Fixes #307

- [#351](https://github.com/klee-contrib/topmodel/issues/351)
  - Implémentation SQL de la gestion des resources:
    - Ajouter la propriété `resourceTableName`
    - Générer le script de création de la table de traduction avec :
      - `resource_key` : correspond à la clé de traduction
      - locale : correspond à la langue traduite
      - label : traduction
    - La pk porte sur les colonnes (`resource_key`, locale)
    - Pour les listes de références, ajouter la FK vers la table de traduction, mais uniquement sur la colonne `resource_key`
    - Ajouter les insertions de traductions, des propriétés et des libellés de listes de références selon le paramétrage

Sur les générateurs qui le supportent (JPA, SQL, JS), il est maintenant possible de surcharger les configurations `translateReferences` et `translateProperties` afin de remplacer les libellés par la clé de traduction correspondant. Il est également possible de générer les traductions correspondantes, chaque générateur ayant ses spécificités à ce sujet.

**Breaking change** : Sans modification de la configuration, dans le générateur SQL, les libellés des listes de références sont maintenant remplacés par leur clé de traduction si le paramètre `translateReferences` est à `true` dans la configuration i18n. **Pour que rien ne change dans le code généré, ajouter à la configuration du générateur SQL `translateReferences: false`.**

## 1.44.0

- [`7574a46`](https://github.com/klee-contrib/topmodel/commit/7574a46b1a3f832bd181af5602f2fb9c0103baa2) - [Core] Ajout du type de flow `HardReplace`, qui effectue la suppression des données dans la table cible **en cascade**

## 1.43.6

- [`803ebd9`](https://github.com/klee-contrib/topmodel/commit/803ebd9bf9eb6d497e811e5669f2ac2110a566c9) - [JPA] Corrections sur les data flows

## 1.43.5

- [`699ae26`](https://github.com/klee-contrib/topmodel/commit/699ae267441bcec5c4c5d226bd39f6a4dbe83d42) - [JPA] Correction sqlName Jdbc pour les associations

## 1.43.4

- [#343](https://github.com/klee-contrib/topmodel/pull/343) - [i18n] Permettre de modifier la génération des fichiers properties java en UTF 8

## 1.43.3

- [`3d2c1f0`](https://github.com/klee-contrib/topmodel/commit/3d2c1f0492f7bbc4c6365a3a694ea717b21e0b6a) - [JPA] Correction mapping dataflow

## 1.43.2

- [#340](https://github.com/klee-contrib/topmodel/pull/340) - [JPA] [Flux de données] ne pas générer colonne pour les attributs @OneToMany

- [#338](https://github.com/klee-contrib/topmodel/pull/338) - [JPA] Import manquant lors de la génération d'une classe étendant une classe d'un autre module

## 1.43.1

- [`1832387`](https://github.com/klee-contrib/topmodel/commit/1832387ea30fbd265281e9c260abb06431386656) - [JS] Fix génération `allComments`

## 1.43.0

- [#336](https://github.com/klee-contrib/topmodel/pull/336) - `primaryKey: true` sur alias

  Les clés primaires ne sont plus implicitement recopiées sur un alias de clé primaire, ce qui permet de pouvoir mettre un `required: false` dessus, où bien de pouvoir définir un alias comme clé primaire sur une classe persistée (avec `primaryKey: true`).

  **petits breaking changes**

  - Les `required: false` sur les PK sont désormais bien pris en compte.
  - Il y a maintenant `@NotNull` sur les alias de PK sur les DTOs en JPA (si pas de surcharge avec `required: false`).
  - Les DTOs ne peuvent plus avoir de PK (implicite), ce qui empêche de faire des associations dessus sans spécifier de propriété (...)

- [`d723e5f`](https://github.com/klee-contrib/topmodel/commit/d723e5fbd98917250a3ac5c1f2eb4af28bb278a8) - [JS] `generateMainResourceFiles`

  Vous pouvez désormais générer un fichier `index.ts` à la racine des traductions côté JS qui réexporte tous les modules dans un seul objet `all` (et `allComments` si vous avez activé la génération des commentaires).

  **minuscule breaking change**

  - Les objets exportés contenant les commentaires s'appellent désormais `{module}Comments` au lieu de `{module}`.

## 1.42.9

- [`76ade23`](https://github.com/klee-contrib/topmodel/commit/76ade23bd1a8d26ca99630041c96faeaa82b53c6) - [C#] Fix génération enum si PK ne peut pas être un enum (genre int)

## 1.42.8

- [#334](https://github.com/klee-contrib/topmodel/pull/334) - [JPA] Permettre de personnaliser l'interface dont héritent les daos + dao abstract

## 1.42.7

- [`6948f3`](https://github.com/klee-contrib/topmodel/commit/6948f37238a4c8d8b2252ef007b5ec0b3fe279f1) - [Core] Mauvaise récupération de la clé primaire dans le cas d'héritage en cascade

## 1.42.6

- [`eef607`](https://github.com/klee-contrib/topmodel/commit/eef6070036f4c97d5ce8159a8f9ef71abe5cac10) - modgen -e pour exclure des tags de la génération

## 1.42.5

- [`2a3b27`](https://github.com/klee-contrib/topmodel/commit/2a3b27654a3c739d15a12c52775733001639e839) - [modgen] correction boucle infinie IsMultipart

## 1.42.4

- [`5b6e6a`](https://github.com/klee-contrib/topmodel/commit/5b6e6a7e5d24aeb4cc4d7a1c8a82aeb72cc173ae) - [JPA] Prise en compte du property casing dans les getters

## 1.42.3

- [`44eb93`](https://github.com/klee-contrib/topmodel/commit/27da8fd072cd4534e43fcee758d737b8e5128bf0) - [SQL] Ne pas générer de script d'insert pour le classes non persistées
- [`56c2dde`](https://github.com/klee-contrib/topmodel/commit/56c2dde501b97cf9fc9ce0228f57a7b2c61c2b3a) - [JPA] Choix du mode de génération de l'api cliente : RestTemplate ou RestClient

## 1.42.2

- [`b141e30`](https://github.com/klee-contrib/topmodel/commit/b141e30665fad3dcc526265ef728c27a9ef8f98f) - [Spring API] Correction bug accept

## 1.42.1

- [`b78f252`](https://github.com/klee-contrib/topmodel/commit/b78f252edc917f1b635b32fa04c1d8514e7579f4) - [C#] Fix espace en trop potentiel dans les contrôleurs au début
- [`e9637ca`](https://github.com/klee-contrib/topmodel/commit/e9637ca77996f36bd8f0c6f80ce9d165e7d3bc6f) - Fix alias dans les décorateurs rerésolus à chaque watch

## 1.42.0

- [#332](https://github.com/klee-contrib/topmodel/pull/332) - [Spring] Génération des api clientes en mode exchange
- [#324](https://github.com/klee-contrib/topmodel/pull/324) - [TS] Mode `Nuxt` pour l'api cliente TS

## 1.41.2

- [`7ab0c37`](https://github.com/klee-contrib/topmodel/commit/7ab0c37668f01dd20c6f553529519ba1b0d3a637) - [C# Mappers] Fix tags utilisés pour mappers avec classes de références

## 1.41.1

- [`f82d037`](https://github.com/klee-contrib/topmodel/commit/f82d037ea86747a9ed7f428cf07daf2e57e4def6) - [JPA] Ne pas ajouter l'annotation id dans le cas composite primary key & jdbc

## 1.41.0

(Cette release n'impacte que le générateur C#)

- [`c96d889c`](https://github.com/klee-contrib/topmodel/commit/c96d889cf0f1128f38c89c8ab1e29fa7cf52b4de) - [C#] Gestion des types (non-)nullables dans le générateur
- [`70015d29`](https://github.com/klee-contrib/topmodel/commit/70015d29b060be8de8877755e8863bc9b2269a7b) - [C# Mappers] Ignore les classes de références persistées (si générées ailleurs) pour déterminer l'endroit de génération d'un mapper
- [`31100a0b`](https://github.com/klee-contrib/topmodel/commit/31100a0b4d7cfc0a8bb27a215e3854923dbaf18a) - [C#] `mapperLocationPriority: non-persistent`
- [`f6c04733`](https://github.com/klee-contrib/topmodel/commit/f6c04733ee3167ae2dfffe3eb2a040818e413f46) - [C#] Mappers : ArgumentException.ThrowIfNull
- [`5ea56dc1`](https://github.com/klee-contrib/topmodel/commit/5ea56dc151169b6ef666c00fd297cedacf289bc5) - [C#] `usePrimaryConstructors`
- [`d9f1284a`](https://github.com/klee-contrib/topmodel/commit/d9f1284a24ccd2b71e0b20e3cf47b6974a2dd322) - [C# ApiClient] Génération d'un fichier partial initial s'il n'existe pas

**breaking changes**

- **impact : moyen** - Les types C# dans les domaines ne doivent plus être indiqués comme nullables (`int?` => `int`). Un simple Ctrl+F "?" => "" dans votre fichier de domaines devrait suffire. Si vous aviez des types non standard à indiquer comme nullables, vous pouvez les lister dans le nouveau paramètre `nonNullableTypes` de la config C#. Dans le code généré, si vous utilisiez bien des types nullables partout (ce qui devrait être le cas), il ne devrait pas y avoir beaucoup d'impacts hormis certains types de paramètres et de retour d'API ou de mappers qui ne devraient plus être nullables s'ils sont obligatoires (ce qui n'était pas toujours le cas avant). Si vous aviez déjà des types non nullables volontairement, désolé mais ils seront nullables maintenant donc il y aura peut être quelques adaptations à faire dans votre code (quelques `.Value` en plus quoi).
- **impact : très mineur** - Du fait de la non-prise en compte des classes de référence dans la localisation des mappers générés, certains mappers pourraient être générés ailleurs, mais cela ne devrait pas être difficile à rattraper (quelques `using static` à changer probablement).
- **impact : très mineur** - Puisque qu'on génère désormais un fichier `.partial.cs` avec chaque client d'API s'il n'existe pas, si votre fichier partial existant n'est pas au bon endroit et avec le bon nom, un fichier partial supplémentaire sera généré à tort. Si c'est le cas, il faudra renommer votre fichier existant pour qu'il corresponde au fichier généré.
- **aucun impact** : La vérification de non nullabilité des paramètres des mappers se fait en une seule ligne avec `ArgumentException.ThrowIfNull`

## 1.40.0

- [#328](https://github.com/klee-contrib/topmodel/pull/328) - Propriétés comme paramètres de mapper `from`

  **breaking changes**

  - Il n'est plus possible de spécifier `this` dans un mapping explicite de classe dans un mapper `from`, il faut utiliser une composition à la place.

    Par exemple :

    ```yaml
    mappers:
      from:
        - params:
            - class: MyDto
            - class: MySubDto
              mappings:
                Sub: this
    ```

    devient

    ```yaml
    mappers:
      from:
        - params:
            - class: MyDto
            - property:
                composition: MySubDto
                name: Sub
                comment: Instance de 'MySubDto'
    ```

    Cela ne fonctionne qu'en génération C#, le générateur JPA ignore les mappings de propriétés.

  - Les paramètres `required: false` dans les mapper `from` doivent maintenant être placés **après tous les autres paramètres obligatoires**. En C#, le code généré était invalide dans ce cas donc c'est maintenant forcé côté modèle. Si vous êtes en Java et que vous n'aviez pas respecté cet ordre, il suffit de changer l'ordre des paramètres dans le modèle puis dans votre code.

## 1.39.9

(Corrige le numéro de version qui incluait le hash du dernier commit suite à la montée de version du SDK .NET)

## 1.39.8

- [#327](https://github.com/klee-contrib/topmodel/pull/327) - Version .NET 8
- [`a8aa2234`](https://github.com/klee-contrib/topmodel/commit/a8aa2234a20b0adbbb1652a7057233578e4482dc) - [C# APIClient] Fix trim manquant sur returnType string

## 1.39.7

- [`1994aed7`](https://github.com/klee-contrib/topmodel/commit/1994aed73e1be603c9028ee74a41f0c19d7a627a) - [C# endpoints] required sur returns + gestion string dans les clients

## 1.39.6

- [`dbadf6cb`](https://github.com/klee-contrib/topmodel/commit/dbadf6cbaaa189a403a24aceb6753edc5e3ec318) - Mise à jour libs .net (permet au générateur d'API server C# de comprendre les constructeurs principaux de C# 12, entre autres)
- [`1969a0f3`](https://github.com/klee-contrib/topmodel/commit/1969a0f3046eaef954138f5a3d8e30eb4162ba87) - [C# ClassGen] Fix using manquant pour alias de PK enum

## 1.39.5

- [`1e37d6e8`](https://github.com/klee-contrib/topmodel/commit/1e37d6e83237d9a325bba7692e9a162f438fcceb) - [C#] Fix import column manquant des fois

## 1.39.4

- [`cb8ada08`](https://github.com/klee-contrib/topmodel/commit/cb8ada08472eb5d3cc281760f351a38c5566a7a3) - [Core/C#] Fix génération SqlName pour alias persistés d'associations + pose de l'annotation Column manquante en C#

## 1.39.3

- [`04b6ac06`](https://github.com/klee-contrib/topmodel/commit/04b6ac066e7f24e18abbad9c99a5da6a3033ed6d) - [SQL] Possibilité de s'auto-référencer dans une enum

## 1.39.2

- [`1f80d97c2`](https://github.com/klee-contrib/topmodel/commit/1f80d97c24bad8cda81dc3f2ad079569104060bc) - [Angular] Correction fullroute dans le cas multipart

## 1.39.1

- [`72a5c3c10b`](https://github.com/klee-contrib/topmodel/commit/72a5c3c10b99d13de066951f39efa10100bfdcd7) - [JPA] Correction génération multipart

## 1.39.0

> **Breaking Change** Précédemment, la génération des endpoints s'appuyait sur le type du domaine pour savoir si elle devait être `multipart`. Il faut désormais préciser sur les domaines de fichiers : `mediaType: multipart/form-data`.

## 1.38.4

- [`edfdc21`](https://github.com/klee-contrib/topmodel/commit/edfdc211d0f714a866a2a2f172552ef73ebfab9b) - [JPA] Correction visibilité flows

## 1.38.3

- [`edfdc21`](https://github.com/klee-contrib/topmodel/commit/edfdc211d0f714a866a2a2f172552ef73ebfab9b) - [JPA] Correction champs nulls nullables dans les mappers, associations réciproques mal générées
- [`267f3e23`](https://github.com/klee-contrib/topmodel/commit/267f3e23b20ee41e02239ffae5d62235ac9a800c) - [JPA] Améliorations syntaxiques sur le code généré

## 1.38.2

- [`405e85105`](https://github.com/klee-contrib/topmodel/commit/405e851054882d805d9100184a8e0688ab6801c8) - [JPA] datflows: récupérer les propriétés du parent dans le mapping

## 1.38.1

- [`fbcb0cd1`](https://github.com/klee-contrib/topmodel/commit/0a850aa2b8127f5626e2f930107f3d5a2ec8609a) - [JPA] Correction bug import enum clé composite
- [`5bcb41123`](https://github.com/klee-contrib/topmodel/commit/0a850aa2b8127f5626e2f930107f3d5a2ec8609a) - [JPA] Correction warning utilisation module dans config flows

## 1.38.0

- [`#319`](https://github.com/klee-contrib/topmodel/pull/319) [Core] Héritage étendus aux `mappers`, `values`, `defaultProperty` et classes persistées
- [`55366e9ad6`](https://github.com/klee-contrib/topmodel/commit/55366e9ad61d322deadf818ac6931f90b5c95246) - [Core] Gestion de l'héritage dans les classes persistées
- [`15b94151bb`](https://github.com/klee-contrib/topmodel/commit/15b94151bb0412988199f24087db6ef066a74352) - [JPA] Gestion de l'héritage dans les classes persistées
- [`49972c10f`](https://github.com/klee-contrib/topmodel/commit/49972c10f9faa21326f7f600fe8a427b4dd34685) - [Core] Association sur des classes dont la PK est dans le parent (avec le mot clé `extends`)
- [`0a850aa2b8`](https://github.com/klee-contrib/topmodel/commit/0a850aa2b8127f5626e2f930107f3d5a2ec8609a) - [JPA] Ajout des getters et setters sur les clés primaires composites

### Classes persistées héritées

Il existe plusieurs mode de stockage pour les objets contenant de l'héritage. Dans la plupart des cas étudiés, le mode le plus pertinent est le mode `join`, où chaque classe possède sa propre table, ne contenant que les informations minimum.

Ainsi :

- La table correspondant à la classe parente correspond exactement à sa représentation sans héritage
- La table enfant contient tous les champs qui lui sont spécifiques, mais aussi un champ qui a une contrainte de clé étrangère vers la table parente. En l'absence d'autre clé primaire dans la classe, ce champ sera sa clé primaire (attention : votre ORM vous imposera probablement de mettre ou non une clé primaire explicite sur la table enfant).

Si votre ORM gère l'héritage (Hibernate, EF Core...), alors la sauvegarde d'un objet enfant effectuera donc des modifications dans les deux tables. De même, si vous renseignez des `values` sur une classe enfant, il faudra également y renseigner toutes les propriétés de la classe parente.

Les propriétés d'une classe parente peuvent également être référencées en tant que `defaultProperty`, `orderProperty` ou `flagProperty` (ou même `joinColumn` dans les [dataFlows](/model/dataFlows.mddata)), ainsi qu'en tant que propriété cible d'une `association`.

Le code écrit par les différents générateurs correspond à ce mode de fonctionnement, selon les spécificités de chacun.

### Héritage étendus aux mappers

Conjointement à l'évolution précédente, la classe enfant hérite maintenant des différentes propriétés de la classe parente dans les mappers, qui seront incluses dans la constitution des mappings entre les classes (des deux côtés, implicites comme explicites). Il n'y a donc plus besoin de définir des mappers sur les classes parentes pour en avoir sur les classes enfantes (mais ces derniers devront en revanche bien définir tous les mappings de leur parent).

### Values

Les `values` ajoutées dans la classe enfant viendront implicitement compléter la liste des valeurs des champs de la classe parent. Les `enum` de valeurs seront impactées de la manière suivante :

- Pas d'enum générée pour la clé primaire de la classe enfant. En effet le type de la propriété est le même que celui de la classe parent
- Les valeurs des champs ajoutées par la classe enfant sont ajoutées à l'enum de la classe parent

## 1.37.6

- [`becb4dd63`](https://github.com/klee-contrib/topmodel/commit/becb4dd639b62edf2b3ac84d4245dcf1198d3639) - [Core] Fix erreur lorsqu'un champ n'est pas renseigné dans une value alors qu'il fait l'objet d'une contrainte d'unicité

## 1.37.5

- [`9ac0734fe7`](https://github.com/klee-contrib/topmodel/commit/d75c18a8e01a8ba062e52e19d53104e6cfb2d190) - [JPA] Correction de l'import de l'enum dans le cas enum alias

## 1.37.4

- [`d75c18a8e0`](https://github.com/klee-contrib/topmodel/commit/d75c18a8e01a8ba062e52e19d53104e6cfb2d190) - [JPA] Correction de l'import de l'enum dans le cas enum shortcut

## 1.37.3

- [`dcafd57f8`](https://github.com/klee-contrib/topmodel/commit/dcafd57f88ce8a0fa4c7ad73ac7f7c805c3c4e02) - [JPA] Correction de la génération du classe ID dans le cas des clés composites

## 1.37.2

- [`0b12c24`](https://github.com/klee-contrib/topmodel/commit/0b12c24e52f8c4528078b54265bc85cb5c8058af) - [Core] La génération du schéma met à jour le fichier de config

## 1.37.1

- [`a7ec5b7`](https://github.com/klee-contrib/topmodel/commit/a7ec5b704b2b5d15bdc5c9bf553f2a6b6b2a2e1c) - [Jpa] Correction régression lorsque plusieurs clés primaires

## 1.37.0

- [`b1b42ec8f`](https://github.com/klee-contrib/topmodel/commit/b1b42ec8f270d717c288d7f027d4a20268e7d6c5) - [Jpa] Mode Jdbc correction d'import JPA.
- [`31df881f2`](https://github.com/klee-contrib/topmodel/commit/31df881f21d186bd961a44e4240ef3b5db998270) - [Jpa] Mauvais type lorsqu'une contrainte d'unicité est ajoutée sur une liste de ref.
- [`#311`](https://github.com/klee-contrib/topmodel/pull/311) [Core] Résoudre en cascade les template & variables globales dans les templates

## 1.36.5

- [`320992ad`](https://github.com/klee-contrib/topmodel/commit/320992ad685150647adca250d07228af1d4110f8) - [Jpa] Implémentation annotations api

## 1.36.4

- [`d3f3a426`](https://github.com/klee-contrib/topmodel/commit/159efe90c1a74858ba09a1c96c4b307458fe4c80) - [Jpa] Data flows ajout des partials

## 1.36.3

- [#300](https://github.com/klee-contrib/topmodel/pull/300) - DataFlows hooks
  - Ajout des `hooks`
  - Implémentation spring-batch des `hooks`
- [`769bfa58b`](https://github.com/klee-contrib/topmodel/commit/769bfa58b1ea78749a7607efbe5e3a10749797fc) - [Jpa] Bug fixs sur les imports des types d'enum.
- [`159efe90`](https://github.com/klee-contrib/topmodel/commit/159efe90c1a74858ba09a1c96c4b307458fe4c80) - [Jpa] Ajout de la notion de listener

## 1.36.2

- [`51fb64d3`](https://github.com/klee-contrib/topmodel/commit/51fb64d37543f173bb38925d779ed97b588b4020) - [C#] params de doc sur plusieurs lignes + gestion api cliente qui renvoie byte[]

## 1.36.1

- [`994a9a99`](https://github.com/klee-contrib/topmodel/commit/994a9a9996a415f7195136026d375b422683fd79) - [C#] Fix divers sur les noms d'enums autorisés + et la génération des summarys

## 1.36.0

- [#297](https://github.com/klee-contrib/topmodel/pull/297) - Templates de valeurs par implémentation de domaine

  **Breaking changes**

  - **[C#]** Le générateur C# n'essaie plus de gérer Guid et DateOnly/DateTime tout seul, il faut spécifier les templates correspondants dans les domaines
  - **[JS]** Tous les imports JS renseignés dans topmodel (hors modèle), donc `domainPath`/`fetchPath` et les différents `imports` des domaines sont désormais toujours relatifs au répertoire de génération `outputDirectory` (c'était bien le cas pour les deux premiers déjà), et on considère que c'est un chemin relatif s'il commence par un `.` (au lieu de dire que ce n'en est pas un s'il commence par un `@`)

## 1.35.3

- [`0f28f21c`](https://github.com/klee-contrib/topmodel/commit/0f28f21ce0a8fa9ecfd24c3e1d68de16cb24a1c9) - [C# DbContext] Fix tag utilisé pour les usings de classes

## 1.35.2

- [#295](https://github.com/klee-contrib/topmodel/pull/295) - `ignoreDefaultValues` + gestion standard des (default)Values en SQL

## 1.35.1

- [`0d4bc60a`](https://github.com/klee-contrib/topmodel/commit/0d4bc60a296a63d8c4090069609454e0c9838aef) - [C#] Gestion "native" (default)Value en Guid (et dates en dehors du DbContext)

## 1.35.0

- [`6ce702`](https://github.com/klee-contrib/topmodel/commit/6ce7020f672fec50f23c8a29d45e9ac1d1a95868) [JS] Les primary key non auto-générées sont désormais obligatoires
- [`9ea8a4`](https://github.com/klee-contrib/topmodel/commit/9ea8a4251b6491608e84e731b214f03631d0212c) [JS] Fix listing références des domaines utilisés en `asDomains`
- [#292](https://github.com/klee-contrib/topmodel/pull/292) [Core] Donne la possibilité de mettre des paramètres de domaines

Breaking change :

> [JS] Les primary key non auto-générées sont désormais obligatoires

## 1.34.1

- [`481ef`](https://github.com/klee-contrib/topmodel/commit/481ef89b568504199c1573c3999eab17e6f3c3bf) [JS] Correction régression génération config

## 1.34.0

- [#286](https://github.com/klee-contrib/topmodel/pull/283) Spring-batch dataFlows, mode JDBC et séparation des values/enums
  - Implémentation des data flows avec spring batch
    - Génération des Reader, Processor, Writer
    - Orchestration avec génération des steps
  - Ajout d'un mode JDBC pour la génération des classes persistées Java
  - Refactorisation des enums générées
    - Séparation de l'enum des valeurs possibles de la clé primaire, et des valeurs possibles de l'objet
    - Ajout de la configuration de l'emplacement de génération des enums de clé primaire
    - Ajout du constructeur par PK (pour les enums)
    - Suppression de l'enum `Values`
      - La méthode `getEntity` n'existe plus
    - Ajout des instances correspondant aux values en tant que membres statiques de la classe

**Breaking Changes (JPA) :**

- Suppression de la classe imbriquée `[Nom de la classe].Values` pour les listes de ref
  - En remplacement du `getEntity`, récupérer l'instance soit avec le constructeur `new [Nom de la classe]([élément du type de la clé primaire])`, soit directement avec l'instance statique de la classe `[Nom de la classe].[Clé de l'instance]`.
  - En remplacement du type de la clé primaire, une enum `[Nom de la classe][Nom de la clé primaire]` est générée au chemin spécifié dans la config : `enumsPath`

## 1.33.1

- [`10c1d1`](https://github.com/klee-contrib/topmodel/commit/10c1d166017f3ca9115d185123da8db5aa80d33a) [JS] Correction génération du fichier de resources dans le cas où une liste de référence a `enum: false`
- [`82afb8`](https://github.com/klee-contrib/topmodel/commit/82afb813331a2a5168cc2c16514884534ca0c93d) [Angular] Génération angular dans le cas `string` et `post`
- [`25e0d2`](https://github.com/klee-contrib/topmodel/commit/25e0d22984e908865d9257b149bac8e4b4ac382e)
  - Message d'erreur converter inversé
  - Génération angular dans le cas `string` et `get`
  - Meilleure utilisation du converter JPA
  - Amélioration de la génération api client spring

## 1.33.0

- [#286](https://github.com/klee-contrib/topmodel/pull/286) - Retrait alias de classes et d'endpoints et ajout de tags supplémentaires

  **breaking change** : Il n'est plus possible de définir des alias de classes et d'endpoints. A la place, il suffit de renseigner la propriété `tags` directement sur les classes et les endpoints avec les tags que vous auriez mis sur les fichiers qui définissaient ces alias. **Cette fonctionnalité n'avait rien à voir avec les alias de propriétés que vous utilisez partout**.

- [`3a2d2177`](https://github.com/klee-contrib/topmodel/commit/3a2d2177668b248e3481707a9911b45520ad3afb) - Surcharge du nom d'une propriété dans un alias

## 1.32.3

- [`26ce249f`](https://github.com/klee-contrib/topmodel/commit/26ce249f1baef13ba554967c08a274d75745e655) - [C# - classgen] Ajout implémentation pour "IList"
- [`af8f506c`](https://github.com/klee-contrib/topmodel/commit/af8f506ca1c908e61c73c69d9048db35fdc5e069) - Fix maj libellés en mode watch
- [`105c6ee3`](https://github.com/klee-contrib/topmodel/commit/105c6ee3d27bedee2fc936a5fb2bccf4e48a3a1d) - Fix watch sur la génération d'endpoints en plusieurs fichiers
- [`38c3ed80`](https://github.com/klee-contrib/topmodel/commit/38c3ed80c883bd0137cc806ca29bb55d1462118e) - Fix initialisation value sur une association required toujours required

## 1.32.2

- [`31ee8378`](https://github.com/klee-contrib/topmodel/commit/31ee83784fe46704bf2809c912491a2e7146b7fe) - [C#] Fix ordre génération consts enums

## 1.32.1

- [`1193b817`](https://github.com/klee-contrib/topmodel/commit/1193b8176daa47a1448fc95ada1b90fbeec3f5e5) - Fix PascalCase pour const enums

## 1.32.0

- [#276](https://github.com/klee-contrib/topmodel/pull/276) - [JS] Séparer le API mode de l'Entity Mode

  **Breaking changes (JS) :**

  La configuration du mode `angular` pour la génération de l'API client JS évolue. Il faut maintenant distinguer le mode de génération de l'API, et le mode de génération des types des entités. Un configuration équivalente au `targetFramework: angular` est donc :

  ```yaml
  targetFramework: angular
  ```

  devient :

  ```yaml
  entityMode: untyped # ou "typed" pour retrouver les types d'entités type "focus" (valeur par défaut)
  apiMode: angular # ou "vanilla" pour avoir des clients en JS purs (valeur par défaut)
  ```

  De plus, **les `StoreNode` ne sont plus générés**. En effet, ils sont spécifiques à l'implémentation Focus et ne sont pas utiles dans le cas général. Il est possible de remplacer par `StoreNode<XXXEntityType>` comme ce qui est déjà fait pour `FormNode`.

- [#262](https://github.com/klee-contrib/topmodel/pull/262) - [JPA] Suppression des constructeur par recopie et des constructeurs tous arguments

  **Breaking changes (JPA) :**

  Les constructeurs **tous arguments** et **recopie** ont été supprimés. Pour retrouver un comportement similaire aux précédentes versions :

  - Créer un mapper `from` ayant pour unique paramètre la classe courante. Cela permettra de générer un constructeur par recopie

    ```yaml
    class:
      name: Demande
    [...]
    mappers:
      from:
        - params:
            - class: Demande

    ```

  - Pour les utilisateurs de `Lombock`, ajouter un décorateur contenant l'annotation `@AllArgsConstructor` sur les classes sur lesquelles un constructeur tous arguments est nécessaire

    ```yaml
    decorator:
    name: AllArgsConstructor
    description: Ajoute l'annotation @AllArgsConstructor de lombok
    java:
      annotations:
        - AllArgsConstructor
      imports:
        - lombok.AllArgsConstructor
    ```

  - Pour les autres, utiliser le constructeur vide et les setters...

- [#263](https://github.com/klee-contrib/topmodel/pull/263) + [C#] Suppression de la génération des constructeurs + `useRecords`

  **Breaking change (C#) :**

  **Le générateur C# ne génère plus aucun constructeur** (ainsi que les méthodes partielles `OnCreated`). Si vous utilisiez `OnCreated` pour une initialisation personnalisée dans un constructeur, vous pouvez toujours définir un constructeur dans la classe partielle à la place.

  Si vous utilisez le constructeur de copie, vous pouvez :

  - Recopier les anciens constructeurs dans une classe partielle (en retirant `OnCreated`) -> solution sans changer l'utilisation des classes
  - Définir un mapper `from` depuis la classe elle-même sur la classe, et l'utiliser à la place (puis assigner les propriétés à changer).
  - Passer la génération des classes en mode `record` (via le nouveau paramètre `useRecords`) et bénéficier du constructeur de copie auto-généré par le compilateur C# (qui s'utilise avec `with`, par exemple : `var instance2 = instance1 with { Property1 = "test" }`))

- [#279](https://github.com/klee-contrib/topmodel/pull/279) - Flux de données et MVP générateur C#

  (Nouvelle fonctionnalité en cours de finalisation, la communication officielle se fera plus tard...)

- [`ec998a52f8`](https://github.com/klee-contrib/topmodel/commit/ec998a52f8e9df4e250dc485987271dd14110ee0) - [C#Class] Retrait enum cols si kinetix = false

- [`55fca66ac2`](https://github.com/klee-contrib/topmodel/commit/55fca66ac22415ad87866ce845b8bc0bc6bab50a) - [JS] Gestion du cas où la liste de référence contient un number ou un boolean

- [#280](https://github.com/klee-contrib/topmodel/pull/280) - [Angular] Génération des appels aux endpoint multipart pour upload un fichier avec un formData

## 1.31.8

- [`3a8fb5effa`](https://github.com/klee-contrib/topmodel/commit/3a8fb5effaf09d7cd41782f1bca5287cb6a4aef6) - [JPA] Fix import mapper

## 1.31.7

- [#278](https://github.com/klee-contrib/topmodel/pull/278) [JPA] Ajout d'un mode `useLegacyAssociationCompositionMappers` pour rétro compatibilité avec les projets qui utilisaient beaucoup les mappers entre compositions et associations multiples. Cette propriété s'ajoute à la configuration générale, afin qu'elle ne créé pas d'erreur
- [`b21d89fa6`](https://github.com/klee-contrib/topmodel/commit/b21d89fa645574fcb15790eb0b76e17a9d7e3933) - [C#] Fix import en trop pour associations si kinetix = false et enum = false
- [`e3fd9f995`](https://github.com/klee-contrib/topmodel/commit/e3fd9f99513ce248ded775cabc4da70b70fecaef) - [C#] Fix casse paramètres mappers

## 1.31.6

- [#273](https://github.com/klee-contrib/topmodel/pull/273) - [Core] Gestion des associations vers PK composite (avec `property` explicite).
- [`eea1c147d`](https://github.com/klee-contrib/topmodel/commit/eea1c147da6849e0694e8f967cd9c3b74cf5fe1f) - [Angular] Erreur lorsque le type de retour est "string"

## 1.31.5

- [`b81dac28`](https://github.com/klee-contrib/topmodel/commit/b81dac28cf55406d3af314141b612c1833ae3618) - [#266](https://github.com/klee-contrib/topmodel/issues/266) : [JPA] Ajout des adders & removers
- [`77ec0fe2`](https://github.com/klee-contrib/topmodel/commit/77ec0fe240c7c4eefb2726ec9a2316c5d3606cdf) - Fix casse des classes si tout en majuscule + dans les mappers C#

## 1.31.5

- [`2b456179`](https://github.com/klee-contrib/topmodel/commit/2b456179a1876fdc5cdc1483557010ff657c6009) - [C#] Unification génération des codes pour DbContext + ReferenceAccessor avec "Get(Default)Value"

  **impacts génération (C#)** : Les initialisations de valeurs enum (sans enum C#) dans `HasData` du DbContext (et des accesseurs de références Kinetix si la liste n'est pas persistée) utilisent désormais les constantes si elles existent, au lieu de mettre la valeur en dur.

## 1.31.3

- [`52f5abc2`](https://github.com/klee-contrib/topmodel/commit/52f5abc2d592656d4ff763af2db817d600bbb8ee) - [C#] Fix usings dans les accesseurs de référence pour les classes non persistées

## 1.31.2

- [`c7139454`](https://github.com/klee-contrib/topmodel/commit/c7139454f62964f4da527a063d8cf9ef22c61e3a) - [JPA] Correction case enumShortcut multiple
- [`ed67fdd5`](https://github.com/klee-contrib/topmodel/commit/ed67fdd52f6a2a252e6df764a9e0656a3752a905) - Fix regroupement des endpoints par fileName si le tag ou le module est différent

## 1.31.1

- [`c5f30de9`](https://github.com/klee-contrib/topmodel/commit/c5f30de956d443fbb02cfb6230f55a14316a1ff3) - Fix trigram sur alias qui ne fonctionnait pas
- [`4be45232`](https://github.com/klee-contrib/topmodel/commit/4be4523259c5cf4944b9c64bef974bce957e6024) - Fix plantage moche si plusieurs PK

## 1.31.0

- [#255](https://github.com/klee-contrib/topmodel/pull/255) - Domaines de composition et transformations de domaines explicites

  **breaking changes**

  - Dans un domaine, remplacer `listDomain` par `asDomains: list:`
  - Dans un alias, remplacer `asList: true` par `as: list`
  - Dans une composition, retirer `kind: object`, remplacer `kind: list` par `domain: DO_LIST` (par exemple, vous pouvez utiliser n'importe quel domaine), et remplacer `kind` par `domain` pour les compositions qui utilisaient déjà des domaines
  - Implémenter `genericType` dans les implémentation de domaines pour :
    - Les domaines de compositions (ils n'utilisent plus `type`), et inclure `{T}` dans leur définition (à la place de `composition.name` s'il y était)
    - Les domaines utilisés par `asDomains: list` (`as: list` et associations `toMany`), à priori avec un `{T}[]` ou `List<{T}>` pour correspondre au type du domaine
  - Dans les imports d'implémentation de domaine JS, ajouter le type importé à la fin de l'import derrière un `/`.
  - Les mappings entre associations `toMany` et compositions `list` n'existent plus

  La PR est une excellente lecture pour accompagner ces changements...

  **impacts génération (C#)**

  - Les propriétés dans les constructeurs de copie des classes sont maintenant toutes dans l'ordre de déclaration des propriétés dans le modèle, au lieu de mettre les compositions en premier.

## 1.30.2

- [`35921291d`](https://github.com/klee-contrib/topmodel/commit/35921291d68985b499e9fec06f914d9052f2145b) - [Angular] Correction api client dans le cas des queryParams
- [`#235`](https://github.com/klee-contrib/topmodel/pull/249) - [SQL] Possibilité de générer un fichier de commentaires

## 1.30.1

- [`ce033dd5b`](https://github.com/klee-contrib/topmodel/commit/ce033dd5b056bddfcdb81a59a00c373c32fe33b0) - [modgen] Bloque la génération s'il y a une erreur dans la config d'un générateur

## 1.30.0

- [#249](https://github.com/klee-contrib/topmodel/pull/249) - Génération du schéma de la config via modgen --schema et générateurs personnalisés

  Le schéma JSON complet du fichier de config (pour valider ce que vous écrivez dans le fichier `topmodel.config`) n'est plus fourni par l'extension VSCode mais doit être généré via la commande `modgen --schema`. L'extension peut le faire automatiquement si vous voulez. (la documentation [a été mise à jour](https://klee-contrib.github.io/topmodel/#/generator))

- [#247](https://github.com/klee-contrib/topmodel/pull/247) - [JPA] Gestion des clés primaires composites, même lorsqu'il s'agit d'associations (manyToMany explicite)

- Comme pour la release précédente, il y a eu un gros travail de refactoring réalisé dans le code afin de préparer l'arrivée de nouveaux générateurs (il y a beaucoup de commits...). Un générateur PHP a été développé grâce à ces refactorings mais il n'est pas inclus dans cette release car il n'a pas encore été suffisamment testé.

  **breaking change (C#)**: la release 1.29 a oublié de remplacer `usings` par `imports` dans les annotations d'implémentations C#. Désolé...

## 1.29.1

- [`0b7dde5d`](https://github.com/klee-contrib/topmodel/commit/0b7dde5d2f069913129388f8d6a05fcc31fc2ec8) - Ajout templating sqlName dans les domaines et décorateurs (classes et propriétés)
- [`9229cb47`](https://github.com/klee-contrib/topmodel/commit/9229cb47db7544045066ab07bc424b3f65b61605) - Fix résolution de template pour ne résoudre qu'une seule variable à la fois
- [`9e42cbfd`](https://github.com/klee-contrib/topmodel/commit/9e42cbfd10a5e3bfbbc20fe546d871c04fe053a5) - [C#] Fix annotations persisted à ne pas générer si noPersistence = true
- [`8650ae48`](https://github.com/klee-contrib/topmodel/commit/8650ae489b912f9e51d956dbdb7a8be7d58d5296) - [JPA] Fix génération defaultValue si association ou pas d'enum
- [`a9c2d4bb`](https://github.com/klee-contrib/topmodel/commit/a9c2d4bb337638734f4d16d715b3731ecb104e31) - Refacto writers & associations réciproques

  **impacts génération (JPA)**: Les commentaires sur les associations réciproques n'ont plus de lien de documentation vers la classe cible.

Cette release contient également du refactoring interne ([`d44a8f69`](https://github.com/klee-contrib/topmodel/commit/d44a8f69f79193cdb7a928ae722ff5680fafa2cf), [`5862cd86`](https://github.com/klee-contrib/topmodel/commit/5862cd86c4e200fdb17c7a641a3edf4b6bf65174), [`15ffd8ba`](https://github.com/klee-contrib/topmodel/commit/15ffd8ba929c932d22b71297f29de85bea86cd36), [`06b1a121`](https://github.com/klee-contrib/topmodel/commit/06b1a121d3295db8c7921fa1761538c2fe69ef8a) et [`7428398d`](https://github.com/klee-contrib/topmodel/commit/7428398d6070f1828b69fa427edd4da1eaa4ed87)) qui ne devrait avoir aucun impact dans le code généré hormis le point noté ci-dessus pour le générateur JPA.

## 1.29.0

- [#243](https://github.com/klee-contrib/topmodel/pull/243) - Déspécialisation des domaines/décorateurs/convertisseurs

  **breaking changes**: les implémentations de domaines (et décorateurs/convertisseurs) sont désormais toutes définies selon le même schéma, au lieu d'avoir un schéma spécifique par langage (`csharp`, `java`, `ts` et `sql`). Concrètement :

  - `java` : aucun changement
  - `csharp` : remplacer `usings` par `imports`.
  - `ts` : remplacer `import` par `imports`, qui est une liste (ce n'est pas vraiment possible d'avoir plusieurs imports en JS tout de même)
  - `sql` : remplacer `sqlType` par un objet `sql` avec `type` dedans.

## 1.28.7

- [`96112115`](https://github.com/klee-contrib/topmodel/commit/96112115c4486c91c8deda3c73f6213283a4bf96) - [C#ClassGen] Fix using en trop pour un StringLength si la propriété est une enum C#

## 1.28.6

- [`e2486221`](https://github.com/klee-contrib/topmodel/commit/e248622147fb05268c3d6f74b7b81823c7387222) - [JS] Gestion des imports de classes avec des tags différents

## 1.28.5

- [`4d2ce224`](https://github.com/klee-contrib/topmodel/commit/4d2ce224b664fcd828438058b6e075e152007069) - [Mappers] Fix tri des mappers from dans la génération

## 1.28.4

- [`73d275cd`](https://github.com/klee-contrib/topmodel/commit/73d275cd1658195fcc691ca50707ed347b64e80b) - [modgen/tmdgen] Fix bug détection fichiers de config + mode watch

## 1.28.3

- [`350930b7`](https://github.com/klee-contrib/topmodel/commit/350930b703dad3627f8e346ad10de4add8b9ec91) - [C#] Fix génération liste de ref non persistées avec tag différent
- [`10ce9a6c`](https://github.com/klee-contrib/topmodel/commit/10ce9a6c9156da3f6df00b4285ef8a432bbbadc6) - [C#ApiClient] Fix usings en trop si pas d'enums pour les enum

## 1.28.2

- [`f6615535`](https://github.com/klee-contrib/topmodel/commit/f6615535d4ef349a11dc8735302dfb367d429d84) - [C#] Fix import en trop class gen
- [`18a686e1`](https://github.com/klee-contrib/topmodel/commit/18a686e15e8ea486042cee2f3bcca864d67bf2d9) - [modgen/tmdgen] Cherche le fichier de config dans les répertoires parents si non trouvés dans les enfants

## 1.28.1

- [`c8d53f11`](https://github.com/klee-contrib/topmodel/commit/c8d53f1167142fbbc3a2c7fda17972c9e08b1a9f) - [SQL] Fix FK générées à tort pour des classes non disponibles

## 1.28.0

- [`87957382`](https://github.com/klee-contrib/topmodel/commit/8795738240650d176c7c8a44519bc5fe69e3dacc) - [Mappers] Génération des mappers à côté de la classe persistée au lieu des classes persistées du module de la classe du mapper + [C#] `moduleTagsOverrides`

  **impacts génération (C#/JPA)** : Les mappers (statiques) qui utilisent des classes persistées sont désormais générés dans le module de la (première) classe persistée au lieu du module de la classe qui définit le mapper. Cela ne devrait pas causer de problème majeur, en particulier côté JPA si les mappers sont utilisés via les DTOs.

- [`e7f24d1f`](https://github.com/klee-contrib/topmodel/commit/e7f24d1f37cb2e65e5d597a7064de578cec07ceb) - [C#] "persistant" > "persistent"

  **breaking change (C#)** : j'ai corrigé mes fautes d'orthographe sur `persistentModelPath` et `nonPersistentModelPath` dans la configuration du générateur C# (en anglais ça s'écrit avec un `e` et non un `a`). Désolé.

- [`86231438`](https://github.com/klee-contrib/topmodel/commit/862314387e3bb8c2c18e756120ee976db65277ad) - [C#ClassGen] Annotations de colonnes que si la classe persistée est dans la config

## 1.27.3

- [`f3122fa2`](https://github.com/klee-contrib/topmodel/commit/f3122fa222da55869e7b2653beacb767ec297218) - Erreur claire en cas de doublon de domaine.
- [`60246ee9`](https://github.com/klee-contrib/topmodel/commit/60246ee9ea498ae9e267ea7cd712937826bd054d) - [C#] Fix annotation kinetix référence en trop si classe non disponible
- [`884ebc9f`](https://github.com/klee-contrib/topmodel/commit/884ebc9f5bd54180bdac2d804ed795b33b1a4b08) - [C#] Pas de referenceaccessor si noPersistance: true

## 1.27.2

- [`68ffa085`](https://github.com/klee-contrib/topmodel/commit/68ffa0850ee2882756e8f350b3891ddd0bef8cc1) - [C#] Ajout config `domainNamespace`
- [`6db60654`](https://github.com/klee-contrib/topmodel/commit/6db606549aa6f13008ed4b5d388091f6566d62aa) - [Mappers] Fix génération mappers si les mappers persistés et non persistés sont générés dans le même fichier
- [`08169b8e`](https://github.com/klee-contrib/topmodel/commit/08169b8e420b8896268a4ef017b8c3e5db7ab9fd) - [C#] `noPersistance` par tag

## 1.27.1

- [`c05448b5`](https://github.com/klee-contrib/topmodel/commit/c05448b54d5d47a75a5c63c973c53f4ba8dc324f) - [JS] Fix import de réference dans un endpoint sans enum.

## 1.27.0

- [#231](https://github.com/klee-contrib/topmodel/pull/231) - Modularisation modgen, partie 1
- [#232](https://github.com/klee-contrib/topmodel/pull/232) - Evolutions config JPA + détermination des packages (Java)/namespaces (C#) + disable

**Breaking changes**

Cette release contient principalement du refactoring interne pour préparer des évolutions futures (#spoiler). Ce refactoring se traduit dans des **changements sur les configs** de plusieurs générateurs :

- `proceduralSql`/`ssdt` : les configs de ces deux générateurs ont été fusionnées dans une seule config `sql` avec une section `procedural` et `ssdt`
  Exemple :

  ```yaml
  proceduralSql:
    - tags: []
      outputDirectory: ./sql
      targetDBMS: postgre
      crebasFile: 01_tables.sql
      indexFKFile: 02_fk_indexes.sql
      uniqueKeysFile: 03_unique_keys.sql
  ```

  devient :

  ```yaml
  sql:
    - tags: []
      outputDirectory: ./sql
      targetDBMS: postgre
      procedural:
        crebasFile: 01_tables.sql
        indexFKFile: 02_fk_indexes.sql
        uniqueKeysFile: 03_unique_keys.sql
  ```

- `jpa` :

  - `modelRootPath` + `entitiesPackageName` ont été remplacés par `entitiesPath`
  - `modelRootPath` + `daosPackageName` ont été remplacés par `daosPath`
  - `modelRootPath` + `dtosPackageName` ont été remplacés par `dtosPath`
  - `apiRootPath` + `apiPackageName` ont été remplacés par `apiPath`
  - `resourceRootPath` a été remplacé par `resourcesPath`

  Pour spécifier la partie du chemin de destination qui doit être utilisée pour constituer le nom du package Java, il faut utiliser le séparateur `:`. Il est aussi nécessaire d'inclure la variable `{module}` dans les chemins. Par exemple :

  ```yaml
  outputDirectory: src/main
  entitiesPath: javagen:topmodel/sample/demo/entities/{module}
  # le package sera topmodel.sample.demo.entities.{module}
  # on peut utiliser des `.` et des `/` de manière interchangeable
  ```

  Exemple de migration :

  ```yaml
  outputDirectory: ../back
  modelRootPath: back-dao/src/main/javagen
  entitiesPackageName: back.dao.entities
  daosPackageName: back.dao.daos
  dtosPackageName: back.dao.dtos
  apiRootPath: back-webapp/src/main/javagen
  apiPackageName: back.webapp.controller
  resourceRootPath: back-core/src/main/resources/i18n/model
  ```

  vers

  ```yaml
  variables:
    # Vous n'êtes pas de définir d'utiliser cette variable mais c'est probablement le plus simple.
    modelRootPath: back-dao/src/main/javagen
  outputDirectory: ../back
  entitiesPath: "{modelRootPath}:back.dao.entities.{module}"
  daosPath: "{modelRootPath}:back.dao.daos.{module}"
  dtosPath: "{modelRootPath}:back.dao.dtos.{module}"
  apiPath: back-webapp/src/main/javagen:back.webapp.controller.{module}
  resourcesPath: back-core/src/main/resources/i18n/model
  ```

  - `csharp` : Si vous utilisiez la variable `{app}` dans vos chemins de modèle/api précédée par un `/`, il faut le remplacer par un `:` pour conserver le même namespace pour les classes générées (auparavant, il était déterminé à partir du chemin cible en retirant ce qui précède la variable `{app}`, maintenant ça marche avec les `:` comme en Java).

## 1.26.3

- [`a4b8757b`](https://github.com/klee-contrib/topmodel/commit/a4b8757b2f3846765214b3d61d108aa539496a54) - Contrôle des types de domaines définis par générateur
- [`1d0c9a79`](https://github.com/klee-contrib/topmodel/commit/1d0c9a79c4ec3e72ca565e4eddb8ce2c41c202a5) - [C#] Gestion enum en constantes non string + fix XML dans les commentaires

## 1.26.2

- [`b72ba42`](https://github.com/klee-contrib/topmodel/commit/b72ba423877c5c9fef57e3fefb397ecc5e02aec7) - [JPA] Mapper to dans le cas de composition kind: list

## 1.26.1

- [`25bcdce4`](https://github.com/klee-contrib/topmodel/commit/25bcdce4eced6d41ea20ca64d9bfd3cbfcaebc96) - [Core] Correction message d'erreur listDomain

## 1.26.0

- [`55cfbbda`](https://github.com/klee-contrib/topmodel/commit/55cfbbdaa49f064e80a28b3e66edf266525925c4) - [JPA] Le préfix du getter des Boolean doit être get et non is #200
- [`27ea8085`](https://github.com/klee-contrib/topmodel/commit/27ea8085e20bba0618572d5677489bbd1b5503e0) - [Core] L'erreur concernant les `listDomain` pour les associations `oneToMany` ou `manyToMany` ne se déclanchait pas toujours

  **breaking changes (JPA uniquement)** Les getters de propriétés `Boolean` sont maintenant préfixés par `get` et non `is`

## 1.25.3

- [`6a7caeaa7`](https://github.com/klee-contrib/topmodel/commit/6a7caeaa7087913286eb15db2bef816eb198e352) - [JPA] Correction génération enum static

## 1.25.2

- [`9b91aa`](https://github.com/klee-contrib/topmodel/commit/9b91aac2600f74d4bf0e0aea7fbcd959f4f89729) - [JPA] Correction génération du allArgsConstructor dans le cas enum shortcut

## 1.25.1

- [`d90967a`](https://github.com/klee-contrib/topmodel/commit/d90967aa538a77676c50833bab1ba1f333e4d969) - Correction NPE dans le `toFirstUpper()`

## 1.25.0

- [#225](https://github.com/klee-contrib/topmodel/pull/225) - Génération de classes C#/Java par tag + variables app/module/lang propres + plein de nettoyage

  **breaking changes (JPA uniquement)**

  - Les mappers sont générés par module complet et non par module racine (ex : les mappers du module `Securite.Utilisateur` sont désormais dans `SecuriteUtilisateurMappers` au lieu de `SecuriteMappers`)
  - Le répertoire cible de génération des fichiers de ressources ne supporte plus la variable `{module}` (qui n'était pas le module mais le module racine). On génère toujours un fichier par module racine et par langue, mais tous dans le même répertoire cible.

- [#226](https://github.com/klee-contrib/topmodel/pull/226) - Uniformisation des conventions de nommage dans le code généré

  **breaking change**

  TopModel va désormais convertir les noms de classes, endpoints et propriétés dans la casse du langage cible de façon systématique, au lieu de le faire de temps en temps. Cela veut dire par exemple qu'en C#, tous les noms de propriétés vont être convertis en `PascalCase`, même si la propriété a été déclarée en `camelCase` dans TopModel, et inversement en Java (ce qui était déjà le cas en revanche). De même, si vous avez des noms avec des `_` dans votre modèle (une classe `Profil_utilisateur` ou une propriété `utilisateur_id`), ils seront également convertis de la même façon (en Java par exemple, ça donnerait `ProfilUtilisateur` et `utilisateurId`).

  En théorie, si vos conventions de nommage étaient correctement respectées dans vos fichiers de modèle, ça ne devrait rien changer dans le code généré. Si vous aviez du modèle généré par un autre outil (par exemple `tmdgen`), il est possible que le modèle généré (et donc le code généré) ne respectait pas la casse cible. Avec cette évolution, il va désormais le faire, donc c'est une source de breaking change possible.

  Parfois, il est important de conserver la casse telle qu'elle a été écrite dans le modèle, en particulier si votre modèle est utilisé avec une API externe qui ne respecte pas les conventions de nommage attendues (par exemple une API dont les propriétés de modèle sont en `snake_case` au lieu de `camelCase`). Il est donc maintenant possible de spécifier `preservePropertyCasing: true` sur une classe ou un endpoint pour que les noms de propriétés ne soit pas convertis. `tmdgen` va désormais renseigner cette propriété sur tout le modèle généré, afin de garantir que les noms de propriétés générés seront bien les mêmes que dans le schéma OpenAPI en entrée.

- [`de3809d3`](https://github.com/klee-contrib/topmodel/commit/de3809d3755a08ba4dfa9ae1818682fdbefa9566) - `modgen --check`

## 1.24.0

- Classes abstraites et propriétés readonly :

  - [#190](https://github.com/klee-contrib/topmodel/pull/190) - Core + C#
  - [#218](https://github.com/klee-contrib/topmodel/pull/218) - JPA

  **Breaking change (JPA)** : Pour générer une interface, l'option `generateInterface` sur les décorateurs a été retirée. Il est possible de faire la même chose via une classe abstraite (`abstract: true` dans la classe, et ajouter l'attribut `readonly: true` sur toutes les propriétés).

- Évolutions sur `values` et `enum`s explicites :

  - [#212](https://github.com/klee-contrib/topmodel/pull/212) - Clarification utilisation de `values` et `enum`s explicites
  - [#219](https://github.com/klee-contrib/topmodel/pull/219) - [C#/JS] Values pour clés uniques simples

  **Breaking changes (C#)** :

  - Les constantes générées pour les values avec une clé d'unicité sont désormais suffixées du nom de la propriété.
  - Avec `enumForStaticReferences: true`, on génère désormais une vraie enum pour les propriétés de classe enum qui ont une clé d'unicité simple.

  **Autres impacts génération** (qui ne sont normalement pas des breaking changes)

  - [C#]
    - On génère des constantes pour toutes les clés d'unicité simple d'une classe si elle a des values et qu'on ne peut pas générer d'enum dessus (le cas enum est décrit dans le breaking change juste au-dessus)
    - L'annotation `DefaultProperty` n'est désormais placée que sur les classes `reference`.
  - [JS] Dans `references.ts` :
    - Ne génère plus de type pour la PK si la classe n'est pas une `enum` (le type était auparavant un alias inutile vers le type de la PK)
    - Ne génère plus de définition de référence (le type + l'objet `{valueKey, labelKey}` ou la liste des valeurs) si la classe n'est pas une `reference`
    - On génère un type pour les propriétés de classe enum qui ont une clé d'unicité simple.

  **Remarque** :

  Si vous avez défini une clé d'unicité sur la PK, vous aurez probablement des choses générés en double, et qui peuvent avoir le même nom (les enums C#/unions TS en particulier). Il est inutile de déclarer une clé d'unicité sur une PK donc vous pouvez simplement la supprimer (peut être que topmodel devrait mettre une erreur dessus ?).

- [#208](https://github.com/klee-contrib/topmodel/pull/208) - Utilisation domain list pour oneToMany et ManyToMany

  **Breaking change (JPA)** : Les domaines de propriétés de PK utilisées dans des associations _one to many_ et _many to many_ doivent maintenant spécifier un `listDomain` correspondant

- [#214](https://github.com/klee-contrib/topmodel/pull/214) - [JPA] Génération de mappers statiques

  **Impacts génération (JPA)** : Les mappers sont désormais générés dans des classes statiques, qui sont ensuite appelés dans les constructeurs (mappers `from`) et dans les méthodes `to` (mappers `to`), qui contenaient au prélable les implémentations. Leurs signatures sont inchangées donc cela ne devrait avoir aucun impact sur leur utilisation.

- [#217](https://github.com/klee-contrib/topmodel/pull/217) - Variables par tag pour traductions (JPA/TranslationOut) + DbContext/ReferenceAccessor en C#

- [#196](https://github.com/klee-contrib/topmodel/pull/196) - [JPA] Valoriser le orderProperty dans les associations oneToMany et manyToMany

- [#211](https://github.com/klee-contrib/topmodel/pull/211) - [JPA] Gestion des enums pour des codes non valides en Java

- [`156507f`](https://github.com/klee-contrib/topmodel/commit/05fcaf27163088ab6095f24612e8871582d27d71) - [JPA] Rendre paramétrable le fait de générer les fieldsEnum pour les classes non persistées

  **Breaking change (JPA)** : Pour les utilisateurs de la propriété `fieldsEnum: true`, remplacer `true` par `Persisted`.

- [`81389b7`](https://github.com/klee-contrib/topmodel/commit/81389b71e90cecfa3d46bdb2afd0bce8eb103231) - [SSDT] Support pour oneToMany/manyToMany (idem proceduralSql)

- [`354b869`](https://github.com/klee-contrib/topmodel/commit/354b8697d1f535539416ed33314319dfed76bffb) - **Impact génération** : [C#] Ce commit technique modifie le contenu du summary des clients d'API (d'un truc inutile vers un autre truc un peu moins inutile...).

- [`be8f10e`](https://github.com/klee-contrib/topmodel/commit/be8f10e8dba8f3f5f7cb8a4c20f225d8f6b3ba3d) - [C#] Gestion oneToMany/manyToMany **minimale** (histoire que ça ne fasse pas d'erreurs de génération, mais ce n'est toujours pas, et ne sera jamais, géré par ce qu'on génère pour EF Core)

## 1.23.4

- [#204](https://github.com/klee-contrib/topmodel/pull/204) - Gestion de clés primaires composites pour de vrai

  **breaking changes** :

  - `allowCompositePrimaryKeys` n'existe plus dans la config globale (il est toujours à `true` maintenant).
  - topmodel ne considère plus une classe avec que des associations comme une n-n persistée : il faut explicitement marquer les propriétés comme `primaryKey: true` désormais

  _(pardon cette PR n'était pas censée être déployée avec ce patch...)_

- [`156507f`](https://github.com/klee-contrib/topmodel/commit/156507fe39f6b32a725254656f3174baeab5a1c8) - Ne converti pas un "entier" avec un "0" au début en int lors du parsing pour la vérification du schéma JSON

## 1.23.3

- [`1a9dcb0`](https://github.com/klee-contrib/topmodel/commit/1a9dcb0ecff530b5aed0200b15213be9178b762d) - Possibilité de changer le nom du lockfile
- [`b954b32`](https://github.com/klee-contrib/topmodel/commit/b954b32a918e289cf56bc278da31d59947680ac1) - modgen: return 1 si erreur de génération ou de parsing

## 1.23.2

- [JPA] Correction régression 1.23.0 : NPE sur les mappers, et mauvaise gestion nullabilité dans certains mappers

## 1.23.1

- [`a3e9efc8`](https://github.com/klee-contrib/topmodel/commit/a3e9efc81c23ca2c51df9ce5a0a07f8dba37e406) - Fix parsing yaml int à la place d'un string

## 1.23.0

- [#199](https://github.com/klee-contrib/topmodel/pull/192) - Variables globales et par tags par générateur dans la configuration

  **breaking change**: [JS] `domainImportPath` et `fetchImportPath` ont été remplacés par `domainPath` et `fetchPath`, qui sont des chemins relatifs à `outputDirectory` au lieu de `modelRootPath`/`apiClientRootPath`.

- [#191](https://github.com/klee-contrib/topmodel/pull/191) - Génération des commentaires en JS + décorateurs dans les fichiers de ressources

  **breaking change** : [JS/JPA/TranslationOut] Les traductions générées pour les propriétés issues de décorateurs sont désormais générées pour le décorateur (et donc dans le fichier correspondant à son module, ce qui nécessitera peut être d'ajouter des fichiers à la config i18,) au lieu d'être recopiées sur chaque classe.

- [#192](https://github.com/klee-contrib/topmodel/pull/192) - Surcharge de domaine dans un alias
- [`0a84b7ee`](https://github.com/klee-contrib/topmodel/commit/0a84b7ee5275d5a07b83ff7cf1fc7de8b6588a1a) - [JPA] Fix déclaration d'une colonne déclenche l'ajout de l'annotation `@NotNull`
- [`421a7c23`](https://github.com/klee-contrib/topmodel/commit/421a7c23a1005f3b6b5ee7aa87e85e989e867d05) - [JS] Angular Api client : Fix virgule surnuméraire

## 1.22.1

- [`c216ac80`](https://github.com/klee-contrib/topmodel/commit/c216ac803a517c4c9337ce57d3bd16de3bd4d6a4) - [P-SQL] Fix mise en page et nommage des FKs (SqlName au lieu de Trigram)

## 1.22.0

- [#186](https://github.com/klee-contrib/topmodel/pull/186) - Remplace `asListWithDomain` sur les alias par `asList: true` et `listDomain` sur le domaine (**breaking change**).

## 1.21.0

- [`ca2aaae7`](https://github.com/klee-contrib/topmodel/commit/ca2aaae7843aae405eec147eea12f699509eb71a) - [Core] Ajout de la notion de converter
- [`648267a2`](https://github.com/klee-contrib/topmodel/commit/ca2aaae7843aae405eec147eea12f699509eb71a) - [VSCode] Refactorisation extension

## 1.20.1

- [`5060edfd`](https://github.com/klee-contrib/topmodel/commit/5060edfd013ada7bbf01fa886fe21ad4b74fff3a) - [C#] Séparation des mappers avec moins une classe persistée des autres et retrait de [ReferencedType] pour tout sauf les références

## 1.20.0

- [#176](https://github.com/klee-contrib/topmodel/pull/176) - Valeurs par défaut pour les propriétés

  Les valeurs par défaut (`defaultValue`) sur les propriétés ne sont plus interprétées comme des valeurs par défaut en SQL mais comme des valeurs par défaut sur tout le reste (propriétés de classe, paramètres d'endpoints, en C#/Java/Javascript). C'est un **breaking change** mais à priori la fonctionnalité n'était pas vraiment utilisée de cette façon.

- [`458d0b9d`](https://github.com/klee-contrib/topmodel/commit/458d0b9dcdb0f700b320b290c00c6210dbcd33b6) - [C#] Génération d'enums que si aucune valeur ne commence par un chiffre.
- [`4e39a208`](https://github.com/klee-contrib/topmodel/commit/4e39a208624492b86ba3f9a21a94cbde4847b4ab) - [C#] Maj config générateur (kinetix true/false, useLatestCSharp = true par défaut) (**léger breaking change dans la config**)
- [`aa1d8caa`](https://github.com/klee-contrib/topmodel/commit/aa1d8caa5f7f95421f0ba17a82cb7becba9142e3) - [C#] PersistantReferencesModelPath

## 1.19.9

- [`bb195e52`](https://github.com/klee-contrib/topmodel/commit/bb195e529d9127e522f1758478452a24938af2b9) - [JPA] EnumShortcut : remettre setter originaux et gestion cas associations multiples

## 1.19.8

- [`ec88aea2`](https://github.com/klee-contrib/topmodel/commit/ec88aea2bb29faed3faa808d516b2fccf6611cfb) - [JPA] Correction nom classe API client (PascalCase)

## 1.19.7

- [`d46d76f2`](https://github.com/klee-contrib/topmodel/commit/d46d76f2b37a55677889a67842c2c056390e4eb9) - Fix nom de fichier endpoints préfixés par un XX\_

## 1.19.6

- [`6e832101`](https://github.com/klee-contrib/topmodel/commit/6e832101eccfca44e512d82ccb4f12bd386e799a) - Fix génération client d'API JS en multi fichiers/multi tags

## 1.19.5

- [#175](https://github.com/klee-contrib/topmodel/pull/175) - Fusion des fichiers d'endpoints de même nom et de même module

## 1.19.4

- [`86371afec`](https://github.com/klee-contrib/topmodel/commit/86371afec406dd4115466179c288967a2abeadff) - [JS] : Correction import foireux si liste de ref non accessible

## 1.19.3

- [`cf1af1cb`](https://github.com/klee-contrib/topmodel/commit/a207f380ded20a87ed2e0a59870e5c329e6aab35) - [JS] : Ressources :correction chemin sous-module

## 1.19.2

- [`a207f380d5`](https://github.com/klee-contrib/topmodel/commit/a207f380ded20a87ed2e0a59870e5c329e6aab35) - [JPA] : Condensation des resources par module

## 1.19.1

- [`6f38bf76`](https://github.com/klee-contrib/topmodel/commit/6f38bf76ba8a08b8f0baff6518dfe4f45dbb382e) - [JPA] : Utiliser l'interface dans le return de l'API si elle est disponible

## 1.19.0

- [#172](https://github.com/klee-contrib/topmodel/pull/172) - Décorateurs d'endpoints (fix [#124](https://github.com/klee-contrib/topmodel/issues/124))

- [#173](https://github.com/klee-contrib/topmodel/pull/173) - Donne la possibilité de gérer le multilinguisme dans TopModel

## 1.18.9

- [`163558d2`](https://github.com/klee-contrib/topmodel/commit/163558d298b559ed9ab48c84d139acddbec37f9c) - [C#/JS] Tri déterministe sur les mappers C# et les propriétés de ressources JS

## 1.18.8

- [`76f48ad4`](https://github.com/klee-contrib/topmodel/commit/76f48ad4f6ea74fb2197d6c5463bb4d6bd34fc17) - Ajustement génération pour Linux
  _Remarque : Cette modification va changer le sens des slashs du `topmodel.lock` sous Windows_
- [`e3f5d22a`](https://github.com/klee-contrib/topmodel/commit/e3f5d22ab92a54f2557287376ca9519d94f08f9c) - JPA : inversion protected abstract api client

## 1.18.7

- [`54f04681`](https://github.com/klee-contrib/topmodel/commit/54f04681e10cb124b65b31d841ce28bc60b44963) - [JPA] : Suppression des orphanRemoval

## 1.18.6

- [`4121f67e`](https://github.com/klee-contrib/topmodel/commit/4121f67eebf66414bd9823e45cbe05f30d7aef47) - [JPA] : orphan removal oneToOne

## 1.18.5

- [`45c6077d`](https://github.com/klee-contrib/topmodel/commit/45c6077d4adefbaaf77f80cdd14817f69a132737) - [C# DbContext] Fix définition des commentaires sur les tables
- [`34586b42`](https://github.com/klee-contrib/topmodel/commit/34586b42d2f3ed0ad6fb09796f2be5b7f36733c6) - [C#ServerApiGen] Fix returns en trop si noAsyncControllers + void

## 1.18.4

- [`7791a9de`](https://github.com/klee-contrib/topmodel/commit/7791a9deea0df5ba5114a83f24c40f21fbb90b7e) - [JPA] : API client factorisation `getHeaders`

## 1.18.3

- [`430d9a3b`](https://github.com/klee-contrib/topmodel/commit/430d9a3b2f943) - [JPA] Fix bugs dans le cas des listes de référénces non statiques

## 1.18.2

- [#171](https://github.com/klee-contrib/topmodel/pull/171) - Paramètres de décorateurs
- [`a98ec1bf`](https://github.com/klee-contrib/topmodel/commit/a98ec1bf0b6fb0d7045b13c512a6e526bba44170) - [C#] Fix ordre génération des mappers

## 1.18.1

- [`0b67988c`](https://github.com/klee-contrib/topmodel/commit/0b67988cc61b78d896661bad97173f6b73cd2a70) - [JPA] : Suppression import inutiles
- [`a3fe1b92`](https://github.com/klee-contrib/topmodel/commit/a3fe1b92f3b0e54b184e5d048bb49658561815f7) - [JS] : Ne pas générer les valeurs undefined des listes de ref
- [`0ec63f6e`](https://github.com/klee-contrib/topmodel/commit/0ec63f6eadde8f13a7b60444d159588d21a54826) - [JPA]: Correction typo 'cannot not be null'
- [`ee0ca15e`](https://github.com/klee-contrib/topmodel/commit/ee0ca15e16c2b1c77d022840c535cea438a8e50e) - [C#] Namespace complet pour les enums dans le dbContext si name == pluralName
- [`01a970b7`](https://github.com/klee-contrib/topmodel/commit/01a970b7f780906d7077572d7c066490d4524622) - Fix plantage si mapper avec association non résolue

## 1.18.0

- [#169](https://github.com/klee-contrib/topmodel/pull/169) - `required` sur les paramètres de mapper `from`
- [#170](https://github.com/klee-contrib/topmodel/pull/170) - `useLegacyRoleNames` (fix [#163](https://github.com/klee-contrib/topmodel/issues/163))

## 1.17.2

- [`c314d20c5`](https://github.com/klee-contrib/topmodel/commit/c314d20c559b5c0274483fab9e73a4beb76ab3ec) - [JPA] : Ne pas cascader les onToMany réciproques

## 1.17.1

- [`a43d4a42`](https://github.com/klee-contrib/topmodel/commit/a43d4a424644e753a3369ededb19255b42943722) - Fix erreurs random sur import inutilisé avec des mappings spéciaux (false et this)

- [`c314d20c5`](https://github.com/klee-contrib/topmodel/commit/c314d20c559b5c0274483fab9e73a4beb76ab3ec) - [JPA] : Ne pas cascader les onToMany réciproques

## 1.17.0

- [#166](https://github.com/klee-contrib/topmodel/pull/166) - [JPA] : Ne pas générer de méthode `equals`
- [#167](https://github.com/klee-contrib/topmodel/pull/167) - [Core] :
  - Donner la possibilité de templater les domaines : [voir la documentation](https://klee-contrib.github.io/topmodel/#/model/domains?id=templating)
  - Spécifier le scope d'une annotation et de ses imports dans le domaine : voir [voir la documentation](https://klee-contrib.github.io/topmodel/#/model/domains?id=sp%c3%a9cialisation)

## 1.16.0

- [#162](https://github.com/klee-contrib/topmodel/pull/162) - [JPA] : Respect de la convention de nommage des interfaces #161
- [#160](https://github.com/klee-contrib/topmodel/pull/160) - [JPA] : Mode Spring 3 : utilisation possible de l'API `jakarta` à la place de `javax`
- [`9298c5ec`](https://github.com/klee-contrib/topmodel/commit/9298c5ec0b56098558d35f70e535409603382312) - [Doc] Ajustements sur les domains dans le tutoriel

## 1.15.11

- [`4138e9f6`](https://github.com/klee-contrib/topmodel/commit/4138e9f69e9b941091245927bb5eff3634a9afa9) - [JPA] Ne pas générer de fichier de ressource vide

## 1.15.10

- [`72e8007c`](https://github.com/klee-contrib/topmodel/commit/72e8007c9b33d99cd76302b473624235f0efcb93) - [JPA] Encoder les fichiers de ressource en Latin1

## 1.15.9

- [`a6bcf36f`](https://github.com/klee-contrib/topmodel/commit/a6bcf36f01b0735581c06eb9bcf20330db3c6aec) - [C#RefAccessor] Fix return au lieu de continue (!)

## 1.15.8

- [#158](https://github.com/klee-contrib/topmodel/pull/158) - Génération des fichiers JS selon le(s) tag(s) des fichiers de modèle

## 1.15.7

- [`77476772`](https://github.com/klee-contrib/topmodel/commit/774767723544ab8fb10dbf798ee9491a488b394e) - [C#Gen] Chemins/noms explicites pour ref accessors + DbContext + retrait conventions legacy
- [`8e148d3e`](https://github.com/klee-contrib/topmodel/commit/8e148d3e247998e78fcd87a5153cf525767e2986) - [C#] Génération des commentaires (tables/colonnes) pour migrations EF

Le générateur C# est désormais muni de ces nouveaux paramètres :

- `dbContextName`
- `referenceAccessorsInterfacePath`
- `referenceAccessorsImplementationPath`
- `referenceAccessorsName`
- `useEFComments`

Les 4 premiers explicitent des conventions plus ou moins obscures (qu'ils remplacement).

## 1.15.6

- [`3232a056`](https://github.com/klee-contrib/topmodel/commit/3232a0565828f3da054057ef6043d5aa8eaa91ee) - [C#RefAccessor] Fix usings si première classe non persistée

## 1.15.5

- [`e49c347a`](https://github.com/klee-contrib/topmodel/commit/e49c347a6c630d585b5d8ba08655f65dec2a7bef) - [JPA] : Amélioration gestion classes statiques et cache

## 1.15.4

- [`577c5ab8`](https://github.com/klee-contrib/topmodel/commit/577c5ab889921ea6ca06877ff7c0a9b77cca5d57) - Fix plantage si doublon de classe ou domaine non définie sur une PK de classe avec values

## 1.15.3

- [`9b7f415a`](https://github.com/klee-contrib/topmodel/commit/9b7f415a6d9c13a917acf9cff70068bcc07866df) - [JPA] : Ne pas rendre les listes de ref immutables

## 1.15.2

- [`2c49fa56`](https://github.com/klee-contrib/topmodel/commit/2c49fa56109674d2091e7d9d358b040baca0fe85) - [JPA] : Correction Dao des listes de ref : le type de la clé primaire n'était pas le bon

## 1.15.1

- [`8e64ecda`](https://github.com/klee-contrib/topmodel/commit/8e64ecdaa10d902d8e99b781f6e10bbc7f85c642) - [C#] Fix surcharge nom DbContext

## 1.15.0

- [#157](https://github.com/klee-contrib/topmodel/pull/157) Tous les générateurs définissent maintenant un "outputDirectory" obligatoire, à partir duquel les autres chemins de génération (modèle, api...) sont déterminés. On s'en servira par la suite pour autoriser des références inter-configurations dans les fichiers générés (même racine = généré au même endroit = peut être référencé)
- [`a6e6ab35`](https://github.com/klee-contrib/topmodel/commit/a6e6ab35fd3ea052993ac3fe2123716cca54e23f) [JPA] : Gestion du mode enumShortcut avec des listes de ref multiples
- [`e3a409ef`](https://github.com/klee-contrib/topmodel/commit/e3a409ef05a7b6aba5df6dbdeeff9674da8952e4) [JPA] : Remplacement des PagingAndSortingRepository par JpaRepository
- [`8561ccfd`](https://github.com/klee-contrib/topmodel/commit/8561ccfdcb8f4b6cb7945fd39293dad56480f69f) [JPA] : Correction génération enum lists (les constructeurs n'étaient pas correctement gérés)
- [`d2207792`](https://github.com/klee-contrib/topmodel/commit/d22077927ad46e49bf18e11a17a86680d4881f3e) [JPA] : Les DAOs ne se généraient pas avec le bon type de clé primaire (`long` en dur dans le code)
- [`7cbaa8e3`](https://github.com/klee-contrib/topmodel/commit/7cbaa8e39b41c9619184d3e1b5d7fff33f0e6d8e) [JPA] : Dans les getter de liste de référénces, mettre prefix `is` si la propriété est un boolean
- [`aed0407b`](https://github.com/klee-contrib/topmodel/commit/aed0407bfbe3a52b89c91a5e813d9b74cdc10d4d) [JPA] : Correction NPE setter enumShortcut list

## 1.14.3

- [`1040e99`](https://github.com/klee-contrib/topmodel/commit/1040e99940f981a1aa6b05ab6cc093424a67f6bd) - [JPA] Correction sqlName dans le cas surcharge du trigram dans la pk

## 1.14.2

- [`ca79975`](https://github.com/klee-contrib/topmodel/commit/ca79975663cd95f403419e72965259c30bfa014a) - [C#Gen] Fix génération enums sur les FK
- [`bcd76d4`](https://github.com/klee-contrib/topmodel/commit/bcd76d4485460cfbe0e61ae1a7349c2d3b26d80b) - [Core] : Correction récupération Label dans cas alias d'alias
- [`7ed8ef6`](https://github.com/klee-contrib/topmodel/commit/7ed8ef6c5f7c8409c8b0ced8ccab9a41122b2217) - [JPA] Correction utilisation trigram dans le cas des manyToMany
- [`d9dec2d`](https://github.com/klee-contrib/topmodel/commit/7ed8ef6c5f7c8409c8b0ced8ccab9a41122b2217) - [VSCode] Ne fonctionne pas sur des fichiers en LF si Environment.NewLine = 'CRLF'
- [`d9dec2d`](https://github.com/klee-contrib/topmodel/commit/ca1c1d4c4e05baffeb0055df81c1700c7f01d3c3) - [JPA] Ajout du générateur de resources

## 1.14.1

- [`74c99e99`](https://github.com/klee-contrib/topmodel/commit/74c99e99a7da1e5a923761d98ad5b047b1405414) - Retrait "GO" en trop si SSDT Postgres
- [`047be582`](https://github.com/klee-contrib/topmodel/commit/047be582120ce3196fa58e08725dc3a3ed2795c4) - [C#Gen] Récupération "AppName" propre depuis la config
- [`30fb0349`](https://github.com/klee-contrib/topmodel/commit/30fb0349aa06ee82b291e6f9507242fcc693d50b) - [modgen] Retrait message d'erreur en trop si on lance --version ou --help

## 1.14.0

- [#153](https://github.com/klee-contrib/topmodel/pull/153) - `modgen` avec configs multiples
- [`097cd3af`](https://github.com/klee-contrib/topmodel/commit/cc857e8c06d4ee3a3bb353b1dba5140fc3340899)...[`cb91c9bf`](https://github.com/klee-contrib/topmodel/commit/cb91c9bf7556f23c24467257f5751d6e8baf8254) - Surcharge locale de trigramme (fix [#128](https://github.com/klee-contrib/topmodel/issues/128), revert [#151](https://github.com/klee-contrib/topmodel/pull/151))
- [#154](https://github.com/klee-contrib/topmodel/pull/154) - [JPA] - Enum dans les values d'enum (fix [#152](https://github.com/klee-contrib/topmodel/issues/152))
- [`716191f4`](https://github.com/klee-contrib/topmodel/commit/716191f4e6a5c38eab50e4778aa8f62d9d76490a) - Fix watch avec des alias de classes et endpoints

## 1.13.5

- [#151](https://github.com/klee-contrib/topmodel/pull/151) - JPA/PG : Préfixer les PK dans le cas où les classes associées n'ont pas de trigram #150
- [`4a605e16`](https://github.com/klee-contrib/topmodel/commit/4a605e16c778e035fe529658841a0035c344ffd2) - [C#ApiGen] Gestion des mots clés réservés dans les noms de paramètres
- [`d1bdb2b3`](https://github.com/klee-contrib/topmodel/commit/d1bdb2b3e9f7bbc65d6a52bfc02f630c3ba04602) - homogénéisation snake_case/dash-case

## 1.13.4

- [`96c32208`](https://github.com/klee-contrib/topmodel/commit/96c3220825b392b36abd7cdea19dce163e8fd1e3) - [C#ServerApiGen] Fix génération contrôleur vide s'il n'y a que des alias d'endpoints

## 1.13.3

- [`831711dd`](https://github.com/klee-contrib/topmodel/commit/831711dd83c40f094e92cc51b58d0d703f42077d) - [C#] Fix Distinct() manquant sur DBContext

## 1.13.2

- [`8427cee2`](https://github.com/klee-contrib/topmodel/commit/8427cee2f53f39f6d4d8b24b2020608cf1bf41c8) - [SSDTGen] InitListMainScriptName facultatif
- [`1072f99b`](https://github.com/klee-contrib/topmodel/commit/1072f99b9b5aea2493a75df14e7f007ca5ff98ec) - [SSDTGen] Mode "postgres"

_Remarque : Générer du SSDT pour PostgreSQL n'a évidemment pas de sens, mais le générateur peut servir pour créer des scripts de définitions de tables et d'inserts complets sans tout regrouper et réordonner pour avoir un script général qui fonctionne en une seule fois._

## 1.13.1

- [`e3a8a3f1`](https://github.com/klee-contrib/topmodel/commit/e3a8a3f1d45a3ee8a3d862fcaa4e0360406f30bd) [JPA] : Correction mapping associations - compositions

## 1.13.0

- [#148](https://github.com/klee-contrib/topmodel/pull/148) - Générateur de client d'API pour Angular (fix [#112](https://github.com/klee-contrib/topmodel/issues/112))
- [#149](https://github.com/klee-contrib/topmodel/pull/149) - Mappings sur les compositions (fix [#146](https://github.com/klee-contrib/topmodel/issues/146))

## 1.12.3

- [`bc2207fa`](https://github.com/klee-contrib/topmodel/commit/bc2207faf0a3b7e73c0e2f93e00fbf93d96ec53b) - [JPA] : Génération de toutes les associations réciproques des oneToMany.

## 1.12.2

- [`0b48d2ee`](https://github.com/klee-contrib/topmodel/commit/0b48d2eee444d849fb5ec1cf1aaa44df021e21f9) - Gestion des dates dans "values"

## 1.12.1

- [`1f017a3f`](https://github.com/klee-contrib/topmodel/commit/1f017a3fec6a1ad32596d9b29d797d1cf679a72f) - Hotfix schema.config.json

## 1.12.0

- [#18](https://github.com/klee-contrib/topmodel/pull/143) PG : Génération tenant compte des types d'associations (ManyToMany en particulier)
  - Adaptation de la génération JPA pour coller avec la génération PG
    - Ajout du mode de génération de clé primaire: `sequence`, `none` ou `identity`
    - Ajout du paramétrage du debut et de l'incrément de la séquence
- [#145](https://github.com/klee-contrib/topmodel/commit/9d588ebdc84a0c74cd3bf37c4982032504138cf6) JPA: compatibilité avec Java 11 (`instance of Class classe` n'est disponible qu'à partir de Java 16)
- [#142](https://github.com/klee-contrib/topmodel/pull/142) Prise en compte de length et scale dans le générateur pg
- [#144](https://github.com/klee-contrib/topmodel/commit/c2411a15022237142dd98c2bf1a7bdf2c081b091) JPA : Générer correctement l'entity Java si le name de la clé primaire ne commence pas par une majuscule

## 1.11.3

- [#140](https://github.com/klee-contrib/topmodel/pull/140) - [JPA] Différenciation des configuration de package (fix [#139](https://github.com/klee-contrib/topmodel/issues/139))
- [#137](https://github.com/klee-contrib/topmodel/pull/137) - Ajout du champ mediaType sur le domaine (fix [#133](https://github.com/klee-contrib/topmodel/issues/133))
- [`6639924b`](https://github.com/klee-contrib/topmodel/commit/6639924bebdb3d67ede1bf3796630773bba1bd92) - [C#Gen] Autorise {module} dans le nom de schéma DB + surcharge nom DbContext

## 1.11.2

- [`3dbbe392`](https://github.com/klee-contrib/topmodel/commit/3dbbe3922d15da6dd263fe63a25bd0725f6dc2a4) - [C# ClientGen] Fix vraiment les paramètres Guid cette fois-ci

## 1.11.1

- [`9e3b2b5e`](https://github.com/klee-contrib/topmodel/commit/9e3b2b5e1d63a7746cf355eccad18d2f5e0fe528) - Fix références "asListWithDomain" manquantes + mini fix C# client gen

## 1.11.0

- [#125](https://github.com/klee-contrib/topmodel/pull/125) - Endpoint Prefix et FileName (fix [#123](https://github.com/klee-contrib/topmodel/issues/123))
- [`25bdb766`](https://github.com/klee-contrib/topmodel/commit/25bdb7663d2588ba18b1c6e6b597ed499a82cb2b) - [C#] Fix génération client API avec un guid dans un query param
- [`29ec1c42`](https://github.com/klee-contrib/topmodel/commit/29ec1c42d1cde038cd663062eb2667ac3f70bd46) - [C#/JS] Gestion API avec plusieurs fichiers et des sous objets (avec FormData)
- [#135](https://github.com/klee-contrib/topmodel/pull/135) - JPA : Implémenter générateur Spring Api client (fix [#134](https://github.com/klee-contrib/topmodel/issues/134))

## 1.10.0

- [`0c0a0a2d`](https://github.com/klee-contrib/topmodel/commit/0c0a0a2d807cf243f839eda357f3550946c4086c) - `property` sur `association` pour spécifier la propriété cible (fix [#129](https://github.com/klee-contrib/topmodel/issues/129))
- [`7ca733cb`](https://github.com/klee-contrib/topmodel/commit/7ca733cbee0e524195e4485d0137dfe6b9cffff9) - Retrait de "asAlias" sur les associations (fix [#130](https://github.com/klee-contrib/topmodel/issues/130))

## 1.9.12

- [`60316754`](https://github.com/klee-contrib/topmodel/commit/60316754166386114c629059aa5087e407636949) - [JPA] Enum Shortcut gestion nullité
- [`428891ed`](https://github.com/klee-contrib/topmodel/commit/428891edfd5c9ca08ac0e4e15211aa268441e5fc) - [JPA] Simplification code généré api
- [`8d68d8b4`](https://github.com/klee-contrib/topmodel/commit/8d68d8b49042328b586d63dc830ff33e53ee7c6e) - [JPA] Suppression annotation RestController inutile

## 1.9.11

- [`e9f70af7`](https://github.com/klee-contrib/topmodel/commit/e9f70af7a069d3e265db09046e120a0e59ba6660) - [JS] Homogénéisation imports pour domaines/liste de ref

## 1.9.10

- [`7a04334`](https://github.com/klee-contrib/topmodel/commit/7a043346ad81e3e85e28d3a24d053e9b33785386) - [TSGen] Fix génération types enum dans les cas non standards

## 1.9.9

- [`766095e`](https://github.com/klee-contrib/topmodel/commit/766095e74ec6e87c12a872fee50215eb1feacd89) - [SSDT] Fix PK uuid manquante dans le table type

## 1.9.8

- [`54c3f84`](https://github.com/klee-contrib/topmodel/commit/54c3f844f6104357b5aec2d361ee21e5a1296650) - Fix divers quand autoGeneratedValue est un string (ex: uuid)

## 1.9.7

- [`75a535c`](https://github.com/klee-contrib/topmodel/commit/75a535c4a8365aafb11b5e4116761501723aada0) - Fix warning import non utilisé si non existant
- [`e722d41`](https://github.com/klee-contrib/topmodel/commit/e722d4159f3219ba14b69ad19ff581823e17fc0d) - [#116](https://github.com/klee-contrib/topmodel/issues/116) Fix plantage si classe non résolue dans un mapper
- [`f41f1fd`](https://github.com/klee-contrib/topmodel/commit/f41f1fd1a9b3264804d71d2bf1a67598bef7bd7e) - Fix [#120](https://github.com/klee-contrib/topmodel/issues/120) (erreurs mal positionnées sur les propriétés)
- [`5fae0b1`](https://github.com/klee-contrib/topmodel/commit/5fae0b134c9a7f94df4e02bf8316b403b11800cb) - Warning décorateur non utilisé ([#119](https://github.com/klee-contrib/topmodel/issues/119))
- [`fae6e99`](https://github.com/klee-contrib/topmodel/commit/fae6e9904e71438a330e2c28eb723aa892aaf6b0) - Amélioration détection mappings
- [`26a989b`](https://github.com/klee-contrib/topmodel/commit/26a989bb231dbb29b7f27c3dbb2523ee9ad72481) - [PG]: Suppression des anciens fichiers [#114](https://github.com/klee-contrib/topmodel/issues/114)

## 1.9.6

- [`32601d4`](https://github.com/klee-contrib/topmodel/commit/32601d4a8f27177144a0888223b438cf8f12400d) [JPA] - Ne plus utiliser le stream().toList() car la liste renvoyée est immutable.

## 1.9.5

[#113](https://github.com/klee-contrib/topmodel/pull/113) + [`43c67ef`](https://github.com/klee-contrib/topmodel/commit/43c67efbde63698b01e7c2a117235d94b324ff9e)

- Ajout de commentaires dans les mappers (définition + paramètres de mappers `from`)
- Implémentation C# de l'héritage de mappers

## 1.9.4

[`2547b6`](https://github.com/klee-contrib/topmodel/commit/2547b691ce42d67b2f66caaab6f1f50d43e56b0a) [JPA] :

- Ajout de la propriété `required` dans les requests params
- Passage de `emptyList` à `new ArrayList()` en cas de nullité de la liste

## 1.9.3

[`2547b6`](https://github.com/klee-contrib/topmodel/commit/2547b691ce42d67b2f66caaab6f1f50d43e56b0a) Fix génération topmodel.lock quand lancé depuis un autre répertoire

## 1.9.2

- [#110](https://github.com/klee-contrib/topmodel/pull/110) JPA - Gestion des associations réciproques au sein d'un même package racine

## 1.9.1

[#109](https://github.com/klee-contrib/topmodel/pull/109) Gestion de l'héritage des mappers.

Cette release ajoute la gestion de l'héritage aux **mappers**.

[Plus de détails dans la doc](https://klee-contrib.github.io/topmodel/#/model/mappers).

## 1.9.0

[#106](https://github.com/klee-contrib/topmodel/pull/106)

Cette release introduit la notion de **mappers** entre classes dans TopModel.

Pour une classe donnée, il est désormais possible de définir des mappers pour instancier cette classe à partir d'autres classes (`from`), ou bien pour instancier une autre classe à partir de cette classe (`t`o`). Les mappings entre propriétés sont déterminés automatiquement dès lors qu'une propriété est un alias d'une autre, ou à défaut si les propriétés ont le même nom et le même domaine. Il est évidemment possible de personnaliser le mapping au-delà de ce qui est déterminé automatiquement.

[Plus de détails dans la doc](https://klee-contrib.github.io/topmodel/#/model/mappers).

## 1.8.3

- JPA: Suppression du allocationSize pour optimisation récupération séquence en masse

## 1.8.2

- [#108](https://github.com/klee-contrib/topmodel/pull/108) [VSCode] Améliorations status bar TopModel #104

## 1.8.1

- [#105](https://github.com/klee-contrib/topmodel/pull/107) VSCode : Correction détection numéro de version et autoUpdate #105

> L'extension est maintenant capable de mettre à jour automatiquement TopModel, si le paramètre topmodel.autoUpdate est renseigné à true

- JS : Correction anomalie régression de la v1.8.0

## 1.8.0

- [#101](https://github.com/klee-contrib/topmodel/pull/101) et [5a15a7](https://github.com/klee-contrib/topmodel/commit/5a15a76141ec6ee17680f7629735e0364b8109ea)/[`2f59e4`](https://github.com/klee-contrib/topmodel/commit/2f59e413eb9a00f6a75e9bf9997df30a32d07aa6) - TopModel génère désormais un fichier `topmodel.lock` qui contient la version de TopModel utilisé par la dernière génération ainsi que la liste des fichiers générés. Cela permet :
  - D'avertir si la version de TopModel installée n'est pas celle avec laquelle le modèle a été généré la dernière fois (pour indiquer à l'utilisateur qu'il doit soit mettre à jour sa version de TopModel, soit mettre à jour son modèle s'il y a des breaking changes avec une nouvelle version)
  - De supprimer automatiquement les fichiers qui correspondant à des classes ou modules précédemment générés par TopModel qui ont été retirés du modèle.
- Bug fixes : [aa8bb02](https://github.com/klee-contrib/topmodel/commit/aa8bb02e5c7a4267a12b64663593a4bb2075cb44) et [7a1eb56](https://github.com/klee-contrib/topmodel/commit/7a1eb56139c7d73c5177f2360c0e70a25723aa3c)

## 1.7.1

[`0f161c6`](https://github.com/klee-contrib/topmodel/commit/0f161c6f216642e940da0718467b7cec8aec219b) - Inférence de default/order/flagProperty que si liste de ref + [C#] DefaultProperty explicite

## 1.7.0

[#97](https://github.com/klee-contrib/topmodel/pull/97) et [#98](https://github.com/klee-contrib/topmodel/pull/98)

- Les définitions de clés d'unicité et de valeurs de références utilisent désormais des références de propriétés, comme on utilise déjà par ailleurs pour les alias par exemple. Cela permet de remonter les erreurs de résolution de propriétés directement dans l'IDE au bon endroit, et d'avoir de l'autocomplétion sur les noms de propriétés.
- De même, `defaultProperty`, `orderProperty` et `flagProperty` utilisent désormais également des références de propriétés, et sont renseignées par défaut par une propriété `Label`/`Libelle` (resp. `Order`/`Ordre` et `Flag`), si elle existe sur la classe. Auparavant, certains générateurs utilisaient "en dur" Libelle si defaultProperty n'était pas renseignée.
- La propriété `autoGeneratedValue` a été ajoutée sur les domaines, pour pouvoir identifier proprement les clés primaires auto-générées au lieu d'un mélange de `Domain.Name != "DO_ID"` ou de `Domain.{lang}.Type == "string"` pas très heureux.
- Une nouvelle erreur a été ajoutée pour proprement identifier le besoin d'avoir une clé d'unicité sur un seul champ pour utiliser des valeurs de références en l'absence d'une clé primaire autogénérée.
- Les deux issues [#95](https://github.com/klee-contrib/topmodel/issues/92) et [#96](https://github.com/klee-contrib/topmodel/issues/92) ont été corrigées

### Breaking changes

- Il faut désormais utiliser le vrai nom de la propriété dans les définitions de clés d'unicités et de valeurs de références. Cela concerne les associations, pour lesquelles on utilisait le nom de classe (et du rôle s'il existe) à la place.
- Il faut ajouter la propriété `autoGeneratedValue: true` sur votre domaine `DO_ID` (que vous devez très certainement avoir)
- [C#] Les propriétés `Label`/`Libelle`/`Order`/`Ordre`/`Flag` étant désormais considérées par défaut comme propriétés par défaut/de tri/de flag, la génération des accesseurs de référence s'en voit impactée avec un tri par `Ordre`/`Order` ou `Label`/`Libelle` alors que rien n'a été spécifié dans le modèle. Ce comportement est tout à fait voulu, mais peut entraîner des différences avec le fonctionnement attendu (le tri par défaut de la base de données était par hasard le bon par exemple).

## 1.6.1

- [#92](https://github.com/klee-contrib/topmodel/issues/92) JPA Ajouter des constructeurs par recopie

## 1.6.0

- [#76](https://github.com/klee-contrib/topmodel/issues/76) Core : Ajouter une notion de décorateur de classe
- [#89](https://github.com/klee-contrib/topmodel/issues/89) Ajouter un générateur d'interfaces pour gérer les projections sur les dto
- [#67](https://github.com/klee-contrib/topmodel/issues/67) Garde-fou sur les doublons

## 1.5.7

- [`07aa8982`](https://github.com/klee-contrib/topmodel/commit/07aa898215c64dc99dfa1c1b98b0616f7d50b4af) Combo PLS HotFix : JPA : Générer des alias constructor #84

## 1.5.6

- [`07aa8982`](https://github.com/klee-contrib/topmodel/commit/07aa898215c64dc99dfa1c1b98b0616f7d50b4af) PLS HotFix : JPA : Générer des alias constructor #84

## 1.5.5

- [`07aa8982`](https://github.com/klee-contrib/topmodel/commit/07aa898215c64dc99dfa1c1b98b0616f7d50b4af) HotFix : JPA : Générer des alias constructor #84

## 1.5.4

- [`07aa8982`](https://github.com/klee-contrib/topmodel/commit/07aa898215c64dc99dfa1c1b98b0616f7d50b4af) JPA : Générer des alias constructor #84

## 1.5.3

- [`79305ff`](https://github.com/klee-contrib/topmodel/commit/79305ff8f65f50e1025ebb2b3d3ede32ee6efeaa) Gestion de la biderectionnalité avec choix du sens arbitraire (fix #82) Cas des oneToMany

## 1.5.2

- [`5f379b`](https://github.com/klee-contrib/topmodel/commit/5f379b3bf9b2d243a9af51bb05a5ddf0927bd7d8) Gestion de la biderectionnalité avec choix du sens arbitraire (fix #82)

## 1.5.1

- [`5f379b3b`](https://github.com/klee-contrib/topmodel/commit/5f379b3bf9b2d243a9af51bb05a5ddf0927bd7d8) JPA : Retirer la dépendance à Lombok (fix #77)
- [`4fea1d49`](https://github.com/klee-contrib/topmodel/commit/4fea1d497b848da6780a83fc2f6421401b63f450) JPA : Générer les associations manyToMany avec des tables de correspondances identiques (fix #80)
- [`2268df33`](https://github.com/klee-contrib/topmodel/commit/2268df33cef6d7eb374bbb89cdc146915ba540e5) Warning sur les domaines non utilisés (fix #73)
- [`c83420e7`](https://github.com/klee-contrib/topmodel/commit/c83420e7c7451e007a0258d85b0b4d19cb31dfb3) Warning sur l'ordre des paramètres d'un endpoint (fix #74)
- [`35c04911`](https://github.com/klee-contrib/topmodel/commit/35c04911081166715946564aa38419f4d5997155) Ignore les fichiers vides (fix #75)

## 1.5.0

- [`550179a9`](https://github.com/klee-contrib/topmodel/commit/550179a9fa45b821538231f8426d1fc85711eea6) VSCode : Prévisualisation du schéma UML

## 1.4.2

- [JPA] : Correction import enum suite release 1.4.1

## 1.4.1

- [JPA] : Correction import enum suite release 1.4.0

## 1.4.0

- [`944b659e`](https://github.com/klee-contrib/topmodel/pull/70) [JPA] : Pour les listes de référence, mettre la FK plutôt que la composition #69
- [`2f057ab7`](https://github.com/klee-contrib/topmodel/pull/68) [JPA] : Bug sur les alias d'association #22
- [`ec6709ae`](https://github.com/klee-contrib/topmodel/pull/64) [Core] : Pluriels et types d'associations #64

## 1.3.4

- [`b344ca01`](https://github.com/klee-contrib/topmodel/commit/b344ca01dc7f47c504dfd75d05fc102cff0f4ecb) [JPA] : EqualsAndHash dans hashset #61

## 1.3.3

- [`5828b26e`](https://github.com/klee-contrib/topmodel/commit/5828b26e8a35b23a448ec7352097ad4d042304c7) [JPA] : Mauvaise génération oneToMany #59
- [`5828b26e`](https://github.com/klee-contrib/topmodel/commit/5828b26e8a35b23a448ec7352097ad4d042304c7) [JPA] : Le EqualsAndHashCode devrait se base sur l'ID uniquement #60

## 1.3.2

- [`6cdc19f`](https://github.com/klee-contrib/topmodel/commit/6cdc19f414ec2606df51d173438b2193e1217d21) [JS] Fix API CLient options non utilisé dans le cas de téléversement

## 1.3.1

- [`95ac1b0`](https://github.com/klee-contrib/topmodel/commit/95ac1b025eb9655ea5c5fb07a3954e995f807174) [JS] Fix API CLient dans le cas d'un téléversement de fichier
- [`065ce7d`](https://github.com/klee-contrib/topmodel/commit/065ce7d160660efa39fae23dbd7ba556f615b66e) [JPA] Feat. Upload de Multipart File
- [`927a846`](https://github.com/klee-contrib/topmodel/commit/927a846b11bd6757e7b185cbd9eb9deb26f2ddb1) [Core] Erreur si une propriété est référencée plusieurs fois dans le même al

## 1.3.0

Version d'accompagnement de la release 1.3.x de l'extension VSCode. Contient de nombreuses améliorations internes pour aider le suivi des références de domaines et classes. Aucun impact n'est attendu sur la génération.

Un nouveau warning a été ajouté pour détecter les doublons de trigramme. Les warnings sont désormais désactivables unitairement via la propriété `noWarn` du fichier de config (valable pour la génération et l'extension VSCode).

## 1.2.11

- [`2788644`](https://github.com/klee-contrib/topmodel/commit/27886449dc529aa1a7a4e229efb0892978e65fb9) [C#] Fix génération classe persistée/DbContext avec des alias

## 1.2.10

- [`b127ba6`](https://github.com/klee-contrib/topmodel/commit/b127ba6b51abd3e08c847a5e062f57120afa3ab6) [JS] Ajout du as const si non mode focus

- [`2e5a932`](https://github.com/klee-contrib/topmodel/commit/2e5a932e1a4ffa05ea379dcec4d6cec31f0500bd) [JS] Correction import liste de ref depuis un sous-module

## 1.2.9

- [`fb3ccc6`](https://github.com/klee-contrib/topmodel/commit/fb3ccc6fdcb5e57636848d4e77633b46a1f61bfc) [C#Gen] Mise à jour nom package Kinetix.

## 1.2.8

- [`869f980`](https://github.com/klee-contrib/topmodel/commit/869f980323727add90afbee892fab27a9586dde3) [C#Gen] Encore un fix sur les usings générés à la marge.

## 1.2.7

- [`fb70003`](https://github.com/klee-contrib/topmodel/commit/fb70003c060d27afa9d0ca4f4bfeb3b27ac49eb2) Fix imports en trop en C# si pas de field properties dans le fichier.

## 1.2.6

- [`b648f4f`](https://github.com/klee-contrib/topmodel/commit/b648f4fe93190ecfd607b616b46fe6a1a6a3150f) JPA - Gérer l'héritage [#38](https://github.com/klee-contrib/topmodel/issues/38)

## 1.2.5

- [`3c83c59`](https://github.com/klee-contrib/topmodel/commit/3c83c59dc36ed159f7c60a6a4d630d178ed5b1c6) JPA - Améliorations persistence :

  - OneToOne : ajout du paramètre `optional` dépendant de la propriété `required`
  - OneToOne : ajout de la contrainte d'unicité sur la colonne qui fait l'objet de la oneToOne
  - Listes de références : les colonnes ont `updatables = false`
  - ManyToOne : ajout du paramètre `optional` dépendant de la propriété `required`
  - ManyToMany : Ajout FetchType.LAZY

- [`2596d4f`](https://github.com/klee-contrib/topmodel/commit/2596d4ffbdd09bae0c327ad6efc717fc43dd40a2) Core : Typage des erreurs [#34](https://github.com/klee-contrib/topmodel/issues/34)

- [`7eeff2f`](https://github.com/klee-contrib/topmodel/commit/7eeff2f38e9bc47f0a2f272fb145fdc780b660ed) Template de type pour les domaines de composition

## 1.2.4

- [`8139acc`](https://github.com/klee-contrib/topmodel/commit/8139acc971fa6214164f841ac37df9933283293d) JS - Mauvais import lorsqu'on référérence un type dans un sous-module [#26](https://github.com/klee-contrib/topmodel/issues/26)

- [`2804a70`](https://github.com/klee-contrib/topmodel/commit/2804a70b308ffaf0c8e853655400428242d0c686) Core : Doit remonter une erreur si un import est dupliqué [#24](https://github.com/klee-contrib/topmodel/issues/24)

## 1.2.3

- [`1c783b6`](https://github.com/klee-contrib/topmodel/commit/1c783b66d2bbd73d010a11ababcd9844e7d25af4) JPA - Ajout du cascade type dans les oneToOne et généralisation de l'utilisation des Set.

## 1.2.2

- [`4de0ae7`](https://github.com/klee-contrib/topmodel/commit/4de0ae72a00b35729d90949c57f915db35e066c8) JPA - Fix referenced column dans les associations oneToOne qui ont un rôle

## 1.2.1

- [`b5c7d25`](https://github.com/klee-contrib/topmodel/commit/b5c7d2553c0bea9d0e65df43a360cab817305913) JS - Api client : Correction import des liste de ref si elles se trouvent dans un sous-module

## 1.2.0

- [`192dbf6`](https://github.com/klee-contrib/topmodel/commit/192dbf67e68da23871956d3fc05e667103602d4f) JS - Ajout du mode `VALUES` pour la génération des listes de ref

- [`a839236`](https://github.com/klee-contrib/topmodel/commit/a839236ea7012961f5761e56934502e953c19bce) (Issue [#7](https://github.com/klee-contrib/topmodel/issues/7)) "reset" des alias déjà résolus à chaque appel de "ResolveReferences"

- [`2a5bff1`](https://github.com/klee-contrib/topmodel/commit/2a5bff154869787f2e637bcdc62b9af0af70984c) (Issue [#17](https://github.com/klee-contrib/topmodel/issues/17)) [C#Gen] Utilisation du module pour déterminer le répertoire de génération des APIs

  `apiPath` a été divisé en `apiRootPath` et `apiFilePath`, ce qui permet de gérer proprement les modules dans les répertoires côté serveur (parce qu'il faut insérer un "/Controllers/" entre les deux paramètres). On n'utilise du coup plus le chemin des fichiers de modèles du tout.

- [`9d59eb0`](https://github.com/klee-contrib/topmodel/commit/9d59eb0b6c70f87167f22154f2c04f582ee4658c) (Issue [#17](https://github.com/klee-contrib/topmodel/issues/17)) JS - API dans le répertoire du sous-module

  N'utilise non plus plus le chemin des fichiers de modèle, et un paramètre `apiClientFilePath` a été ajouté gérer les sous-répertoires en fonction du module.

- [`c83b32c`](https://github.com/klee-contrib/topmodel/commit/c83b32c73247b39324194949d9a47317908076fc) (Issue [#13](https://github.com/klee-contrib/topmodel/issues/13)) JS : Mauvais import lorsque les fichiers d'API sont dans des sous répertoires avec une profondeur > 1

## 1.1.1

- [`7a47a3d`](https://github.com/klee-contrib/topmodel/commit/7a47a3d3c347e3896b9aa7e6748b5050e28df0b5) - JS - Fix majuscule dans le nom de package

## 1.1.0

- [`31050e3`](https://github.com/klee-contrib/topmodel/commit/31050e3114fd2808856572946a612545fa15e13b) - JPA-JS : Fix UpperCase in submodules
- [`040abc3`](https://github.com/klee-contrib/topmodel/commit/040abc3183ae26a52d25fae01ca5369a546475df) - JPA : Génération des fields enum
- [`c41c729`](https://github.com/klee-contrib/topmodel/commit/c41c729dd166c22fcbd86a72788196210de26139) - [C#Gen] Maj namespaces Kinetix SQL

## 1.0.1

- [`edb61d1`](https://github.com/klee-contrib/topmodel/commit/edb61d13080cd6d11e1df36c437f8248c0b95309) - SQL - SQL Name

  Le nom des champs en base de données (SqlName) pour les alias persistés de classes persistées prend désormais en compte la valeur du suffixe et du préfixe éventuellement associés à l'alias.

## 1.0.0

Version initiale.
