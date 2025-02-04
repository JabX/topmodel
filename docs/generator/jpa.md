# Jpa Generator

## Présentation

Le générateur JPA peut générer les fichiers suivants :

- Un fichier de définition de classe pour chaque classe dans le modèle.
- Un fichier de définition de classe pour chaque enum.
- Un fichier d'interface DAO `JpaRepository` pour chacune des classes persistées du modèle.
- Un (ou deux) fichier(s) par module avec les mappers des classes du module.
- Un fichier de contrôleur pour chaque fichier d'endpoints dans le modèle, si les APIs sont générées en mode serveur.
- Un fichier de client d'API pour chaque fichier d'endpoints dans le modèle, si les APIs sont générées en mode client.
- Des fichiers de resources contenant les traductions (`label`) du modèle

Sur toutes les classes, interfaces générées, est ajoutée l'annotation `@Generated("TopModel : https://github.com/klee-contrib/topmodel")` pour permettre de retrouver la doc au cas où 😜. Cette annotation peut être masquée avec le paramètre `generatedHint`.

## Génération des classes

Le générateur de classes distingue trois cas :

- Les classes persistées : les classes qui possèdent une propriété avec `primaryKey: true`
- Les classes non persistées
- Les classes abstraites

Les propriétés sont générées sont `private`, du type défini dans le `domain`. Le commentaire leur étant associé correspond au commentaire défini dans le modèle.

Des `getter` et `setter` sont ajoutés automatiquement. Trois constructeurs sont ajoutés par défaut :

- Constructeur vide
- Construteur tous arguments
- Constructeur par recopie

### Classes persistées

Les classes persistées sont générées avec les annotations correspondant à ce qui est paramétré dans le modèle.
Sur la classe :

| Annotation                                          | Paramètre correspondant dans le modèle                                               |
| --------------------------------------------------- | ------------------------------------------------------------------------------------ |
| `@Entity`                                           | Automatique                                                                          |
| `@Table("SQL_NAME")`                                | Automatique                                                                          |
| `@UniqueConstraint`                                 | `unique` : pour chacune des contraintes d'unicité de la classe                       |
| `Cache(usage = CacheConcurrencyStrategy.READ_ONLY)` | si la classe a `reference: true`. La stratégie dépend du `domain` de la clé primaire |

Sur chacune des propriété :

| Annotation                     | Paramètre correspondant dans le modèle                                                                      |
| ------------------------------ | ----------------------------------------------------------------------------------------------------------- |
| `@Id`                          | `primaryKey: true` : sur la clé primaire                                                                    |
| `@Enumerated(EnumType.STRING)` | Sur la clé primaire, si TopModel a détecté qu'il s'agissait bien d'une enum                                 |
| `@SequenceGenerator`           | `primaryKey: true` : sur la clé primaire si `identity: mode: sequence` dans la configuration générale       |
| `@GeneratedValue`              | `primaryKey: true` : sur la clé primaire si `identity: mode: sequence` dans la configuration générale       |
| `@Column`                      | Sur les propriétés qui ne sont ni des compositions, ni des associations.                                    |
| `@OneToOne`                    | `type: OneToOne` sur une associations                                                                       |
| `@ManyToOne`                   | `type: ManyToOne` sur une associations                                                                      |
| `@OneToMany`                   | `type: OneToMany` sur une associations                                                                      |
| `@ManyToMany`                  | `type: ManyToMany` sur une associations                                                                     |
| `@JoinColumn`                  | Sur les associations `manyToOne` et `oneToOne`                                                              |
| `@JoinTable`                   | Sur les associations `manyToMany`                                                                           |
| `@OrderBy`                     | Sur les associations `manyToMany` et `oneToMany` pour lesquelles la classe cible défini une `orderProperty` |
| `@Convert`                     | Sur les compositions. Le converter utilisé est paramétrable.                                                |

Les paramétrages de ces annotations correspondent à ce qui est défini dans le modèle ou dans la configuration, à l'exception de :

- `fetch = FetchType.LAZY` pour tous les types d'associations, pour optimisation des performances
- `cascade = { CascadeType.PERSIST, CascadeType.MERGE }` pour les associations `ManyToMany` et `ManyToMany`
- `cascade = { CascadeType.ALL }` pour les associations `OneToOne`

Par ailleurs, dès lors qu'une association est faite entre deux classes, si :

- Les deux classes ont même package racine
- La classe de destination n'est pas une liste de référence
- L'association n'est pas de type `oneToOne`

alors l'association réciproque sera générée dans la classe cible.

#### ManyToMany

L'association `ManyToMany` réciproque est générée dans la classe de destination. L'association "propriétaire" de la relation est celle déclarée dans le modèle TopModel.

#### OneToMany

L'association `ManyToOne` réciproque est générée dans la classe de destination. L'association "propriétaire" de la relation est **toujours** l'association `ManyToOne`

#### ManyToOne

L'association `OneToMany` réciproque est générée dans la classe de destination. L'association "propriétaire" de la relation est **toujours** l'association `ManyToOne`

#### OneToOne

Pour des raisons de performances, les associations oneToOne réciproques ne sont pas générées.

#### Enum

Sur une classe, lorsque sont remplis les critères suivants :
- La classe a au moins une propriété
- La classe a des valeurs
- La classe n'a pas `enum: false`
- La clé primaire ou, à défaut, la première propriété, remplit les critère suivants :
  - Le domain a `autoGenerated: false`
  - Le type `java` est `string`
  - Toutes les valeurs de cette propriété peuvent être des enums Java

Alors la classe est une `enum`

Il existe deux modes de gestion des enums.

##### Mode par défaut

- Une enum java `[NomDeLaClasse][NomDeLaPropriété]` est créée en utilisant la propriété `enumsPath` de la configuration, dont les valeurs sont les valeurs possibles définies dans le modèle.
- La classe générée est identique à ce qu'elle aurait été si elle n'avait pas été une enum sauf :
  - Le type de la propriété d'enum est l'enum `[NomDeLaClasse][NomDeLaPropriété]`
  - Si la classe est persistée, l'annotation `@Enumerated(EnumType.STRING)` est ajoutée
  - Des membres statiques sont ajoutés à la classe, représentant les différentes valeurs possibles ajoutées dans les `values`
  - Un constructeur prenant en entrée une instance de l'enum `[NomDeLaClasse][NomDeLaPropriété]` est ajouté

Exemple

```java
/**
 * Type de droit.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
@Entity
@Table(name = "TYPE_DROIT")
@Immutable
@Cache(usage = CacheConcurrencyStrategy.READ_ONLY)
public class TypeDroit {

	@Transient
	public static final TypeDroit ADMIN = new TypeDroit(TypeDroitCode.ADMIN);
	@Transient
	public static final TypeDroit READ = new TypeDroit(TypeDroitCode.READ);
	@Transient
	public static final TypeDroit WRITE = new TypeDroit(TypeDroitCode.WRITE);

	/**
	 * Code du type de droit.
	 */
	@Id
	@Column(name = "TDR_CODE", nullable = false, length = 10, columnDefinition = "varchar")
	@Enumerated(EnumType.STRING)
	private TypeDroitCode code;

	/**
	 * Libellé du type de droit.
	 */
	@Column(name = "TDR_LIBELLE", nullable = false, length = 100, columnDefinition = "varchar")
	private String libelle;

	/**
	 * No arg constructor.
	 */
	public TypeDroit() {
		// No arg constructor
	}

	/**
	 * Enum constructor.
	 * @param code Code dont on veut obtenir l'instance.
	 */
	public TypeDroit(TypeDroitCode code) {
		this.code = code;
		switch(code) {
		case ADMIN :
			this.libelle = "securite.profil.typeDroit.values.Admin";
			break;
		case READ :
			this.libelle = "securite.profil.typeDroit.values.Read";
			break;
		case WRITE :
			this.libelle = "securite.profil.typeDroit.values.Write";
			break;
		}
	}
  /// ... Le reste de la génération est inchangé par rapport aux autres classes
}

```
```java
/**
 * Enumération des valeurs possibles de la propriété Code de la classe TypeDroit.
 */
public enum TypeDroitCode {
	/**
	 * Administration.
	 */
	ADMIN,
	/**
	 * Lecture.
	 */
	READ,
	/**
	 * Ecriture.
	 */
	WRITE
}

```

##### Mode `enumsAsEnums`

En ajoutant la propriété `enumsAsEnums: true` à la configuration, le traitement des enums diffère en ces points :

- Une enum java `[NomDeLaClasse]` est créée, dont les valeurs sont les valeurs possibles définies dans le modèle, et contenant toutes les valeurs des propriétés de la classe. Cette enum est générée avec le chemin contenu dans la propriété `enumsPath` de la config, sauf si la variable `enumsValuesPath` est définie.
- Cette enum n'est donc pas une entité
- Les associations vers cette classe ne sont donc plus des associations au sens JPA, mais des colonnes classiques. Elles portent donc l'annotation `Column`, et puisqu'elles sont de type `enum`, elles portent également l'annotation `@Enumerated(EnumType.STRING)`
- Les noms de propriétés restent identiques au mode par défaut, ce qui permet de garder la compatibilité avec les autres générateurs

Exemple : L'entité TypeDroit est une enum dans le mode `enumsAsEnums`. On peut considérer cette enum comme une contraction de l'enum et de l'entité générés dans le mode par défaut.

```java
////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.enums.securite.profil;

/**
 * Enumération des valeurs possibles de la classe TypeDroit.
 */
public enum TypeDroit {
	/**
	 * Lecture.
	 */
	READ("securite.profil.typeDroit.values.Read"), 

	/**
	 * Ecriture.
	 */
	WRITE("securite.profil.typeDroit.values.Write"), 

	/**
	 * Administration.
	 */
	ADMIN("securite.profil.typeDroit.values.Admin"); 

	/**
	 * Libelle.
	 */
	private final String libelle;

	/**
	 * Enum values constructor.
	 */
	private TypeDroit(final String libelle) {
		this.libelle = libelle;
	}

	/**
	 * Getter for libelle.
	 */
	public String getLibelle() {
		return this.libelle;
	}

}

```


#### Classes non persistées

Les classes non persistées sont générées de la même manière que les classes persistées, mais ne reçoivent pas les annotations JPA.

Par ailleurs, elles implémentent toutes l'interface `java.io.Serializable`. Est ajouté la propriété suivante :

```java
  /** Serial ID */
  private static final long serialVersionUID = 1L;
```

De plus, toutes les propriétés `required: true` reçoivent l'annotation `javax.validation.constraints.NotNull` (ou `jakarata.validation.constraints.NotNull` selon la configuration choisie).

Précautions d'emploi :

- Ne pas ajouter d'association (non sens dans un modèle non persisté)
- Ne pas composer avec une entité persitée

Ceci afin d'éviter de mélanger les objets persistés et non persistés. En effet, si votre objet est sérializé, Hibernate risque de charger tout l'arbre de l'objet correspondant

#### Classes abstraites

Pour générer des interfaces à partir d'une classe du modèle, vous pouvez passer la propriété `abstract` d'une classe à `true`.
Ainsi, le fichier généré sera non plus une classe mais une interface ne contenant que des getters pour chacune des propriétés.

Le cas d'usage typique est celui des [projections de Spring JPA](https://docs.spring.io/spring-data/jpa/docs/current/reference/html/#projections).

```yaml
---
class:
  name: IUtilisateur
  comment: Interface de projection
  abstract: true
```

Si certaines d'entre ont `readonly: false`, qui est la valeur par défaut, alors une méthode `hydrate` sera générée, prenant en paramètre toutes les propriétés non `readonly`. Il s'agit d'un `setter` unique. Ce comportement est identique dans les autres langages pris en charge par TopModel.

Exemple :

```java
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public interface IUtilisateurDto {

  /**
   * Getter for id.
   *
   * @return value of {@link topmodel.exemple.name.dao.dtos.utilisateur.UtilisateurDto#id id}.
   */
   long getId();

  /**
   * Getter for email.
   *
   * @return value of {@link topmodel.exemple.name.dao.dtos.utilisateur.UtilisateurDto#email email}.
   */
   String getEmail();

  /**
   * Getter for typeUtilisateurCode.
   *
   * @return value of {@link topmodel.exemple.name.dao.dtos.utilisateur.UtilisateurDto#typeUtilisateurCode typeUtilisateurCode}.
   */
   TypeUtilisateur.Values getTypeUtilisateurCode();

  /**
   * Getter for profilId.
   *
   * @return value of {@link topmodel.exemple.name.dao.dtos.utilisateur.UtilisateurDto#profilId profilId}.
   */
  long getProfilId();

  /**
   * Getter for profilTypeProfilCode.
   *
   * @return value of {@link topmodel.exemple.name.dao.dtos.utilisateur.UtilisateurDto#profilTypeProfilCode profilTypeProfilCode}.
   */
  TypeProfil.Values getProfilTypeProfilCode();

  /**
   * Getter for utilisateurParent.
   *
   * @return value of {@link topmodel.exemple.name.dao.dtos.utilisateur.UtilisateurDto#utilisateurParent utilisateurParent}.
   */
  UtilisateurDto getUtilisateurParent();
}

```

## Génération des DAO

Un fichier d'interface DAO est généré pour chacune des classes persistées du modèle. Cette interface hérite de `JpaRepository`, et est paramétrée pour gérer l'entité correspondante.

**Ce fichier n'est généré qu'une seule fois !!**. Vous pouvez donc le modifier pour ajouter les différentes méthodes d'accès dont vous auriez besoin. C'est tout l'intérêt.

## Génération des mappers

Les mappers sont générés comme des méthodes statiques dans une classe statique. Cette classe rassemble tous les mappers d'un module racine. Elle est positionné dans le package des entités si l'une des deux classes est persistée, et dans le package des Dtos sinon.

_Remarque : le module utilisé pour un mapper est celui de la classe persistée qui a été trouvée, où à défaut celui de la classe qui définit le mapper._

Les mappers `from` sont nommés `create[Nom de la classe à créer]`. Ils prennent en entrée la liste des paramètres d'entrée définis dans le mapper, plus une instance de la classe cible. Si ce dernier paramètre n'est pas renseigné, alors une nouvelle instance de la classe cible sera créée. Sinon, l'instance cible sera peuplée à partir des paramètres d'entrée renseignés.

Il en va de même pour les mappers `to`. A la différence qu'ils s'appellent `to[Nom de la classe cible]`, ou bien du nom défini dans le `mapper`. Dans le cas des mappers `to`, le paramètre source est unique et obligatoire.

Si un paramètre d'entrée obligatoire n'est pas renseigné, l'exception `IllegalArgumentException` est lancée.

Par ailleurs, dans les classes qui définissent le `mapper`, des constructeurs sont générés pour tous les mappers `from`. Une méthode `toXXX` est générée pour chacun des mappers `to`.

## Génération de l'Api Server (Spring)

Le générateur créé des `interface` contenant, pour chaque `endpoint` paramétré, la méthode abstraite `Nom du endpoint`, à implémenter dans votre controller. En effet, cette méthode aura déjà l'annotation `XXXMapping` correspondant au verbe `HTTP` défini dans le `endpoint`.

Pour créer votre API, il suffit donc de créer un nouveau controller qui implémente la classe générée. L'annotation `@RestController` reste nécessaire.

Si le domain du body du `endpoint` défini un `mediaType`, alors il sera valorisé dans l'annotation avec l'attribut `Consumes`. De la même manière pour le domain du paramètre de retour, avec l'attribut `Produces`.

## Api Client (Spring)

### RestClient (spring-web 6+)

Il s'agit du mode par défaut, soit lorsque la variable `clientApiGeneration` vaut `RestClient`.

Le générateur créé alors des interfaces contenant des annotations `XXXExchange`, dont il faudra configurer un bean d'implémentation.

```java
	@Bean
	protected UtilisateurApiClient utilisateurApiClient(UtilisateurApiClient restTemplate) {
		var restClient = RestClient.builder().baseUrl("http://localhost:8080/my-app/api/") //
				.build();
		var adapter = RestClientAdapter.create(restClient);
		var factory = HttpServiceProxyFactory.builderFor(adapter).build();
		return factory.createClient(UtilisateurApiClient.class);
	}
```

### RestTemplate

Pour activer ce mode de génération, positionner la variable `clientApiGeneration` à `RestTemplate`.

Le générateur créé alors des classes abstraites contenant, toutes les méthodes permettant d'accéder aux endpoints paramétrés.

Pour créer votre client d'API, il suffit de créer une classe qui hérite de cette classe abstraite. Pour fonctionner, elle devra appeler le constructeur de la classe abrstaite, en renseignant :

- Le host de l'API
- Une instance de `RestTemplate`

Exemple :

```java
@Service
public class UtilisateurApiClient extends AbstractUtilisateurApiCLient {

  private static final HOST = "http://localhost:8080/my-app/api/";

  @Autowired
  public UtilisateurApiClient(RestTemplate restTemplate) {
    super(restTemplate, HOST);
  }
}
```

Pour appeler l'API utilisateur, injecter le service UtilisateurApiClient. Puis appeler la méthode de votre choix en entrant les différents paramètres, en y ajoutant l'objet HttpHeaders désiré.

```java
@Service
public class UtilisateurService {

  private static final HOST = "http://localhost:8080/my-app/api/";

  private final UtilisateurApiClient utilisateurApiClient;

  @Autowired
  public UtilisateurService(UtilisateurApiClient utilisateurApiClient) {
    this.utilisateurApiClient = utilisateurApiClient;
  }

  public UtilisateurDto getUtilisateur(Long id){
    var headers = new HttpHeaders();
    headers.add("token-securise", "MON_TOKEN_SECURISE");
    return utilisateurApiClient.getUtilisateur(id, headers);
  }
}
```

### FeignClient

Génère le même fichier que dans le mode `Server` de la génération d'API, à la différence près que le suffix est `Api` au lieu de `Controller`, et que l'annotation `@FeignClient` est ajoutée à l'interface.

## Dépendances

### Modèle

Le modèle généré par TopModel dépend d'une api de persistence. Par défaut, c'est l'API de persistence `javax` qui est utilisée, mais le mode `jakarta` est aussi disponible.

La validation elle est gérée par le package `jakarta.validation-api`, dont les imports changent entre la version 2 et la version 3.

#### Javax (spring-boot < v3)

```xml
<!-- https://mvnrepository.com/artifact/javax.persistence/javax.persistence-api -->
<dependency>
    <groupId>javax.persistence</groupId>
    <artifactId>javax.persistence-api</artifactId>
</dependency>

<!-- https://mvnrepository.com/artifact/jakarta.validation/jakarta.validation-api -->
<dependency>
  <groupId>jakarta.validation</groupId>
  <artifactId>jakarta.validation-api</artifactId>
</dependency>
```

#### Jakarta (spring-boot > v3)

```xml
<!-- https://mvnrepository.com/artifact/jakarta.persistence/jakarta.persistence-api -->
<dependency>
    <groupId>jakarta.persistence</groupId>
    <artifactId>jakarta.persistence-api</artifactId>
    <version>3.1.0</version>
</dependency>

<!-- https://mvnrepository.com/artifact/jakarta.validation/jakarta.validation-api -->
<dependency>
  <groupId>jakarta.validation</groupId>
  <artifactId>jakarta.validation-api</artifactId>
</dependency>
```

### Endpoints

Actuellement, la seule génération de endpoint cliente et serveur qui est gérée passe par les API de `Spring-web`

```xml
<!-- https://mvnrepository.com/artifact/org.springframework/spring-web -->
<dependency>
    <groupId>org.springframework</groupId>
    <artifactId>spring-web</artifactId>
</dependency>
```

### Version Java

Le cde Java généré est compatible avec toutes les versions de Java postérieures à `Java 11`.

## Utilisation combinée avec le générateur postgresql

Le mode de génération par défaut des générateur ne créé par de séquence, mais des colonnes auto-générées avec `identity`. Malheureusement, le `batch insert` de jdbc ne fonctionne pas correctement avec ce mode de génération d'ID. Il est donc recommandé d'utiliser le mode `sequence` de du générateur postgresql.

Le mode `sequence` dans la configuration jpa et dans la configuration postgresql se déclare de la même manière :

```yaml
## Configuration jpa et proceduralSql
identity:
  increment: 50
  start: 1000
  mode: sequence
```

## FieldsEnum

Il est possible de générer dans la définition de la classe, la sous-classe (qui est une enum) `Fields`. Il s'agit d'une enumération des champs de la classe, au format const case.
Il faut pour cela compléter la propriété `fieldsEnum:` A la configuration JPA. Sa valeur détermine dans quelles classes le générateur doit ajouter une enum des champs : aucune (`None`), dans les classes persistées (`Persisted`), dans les classes non persistées (`Dto`), ou les deux (`Persisted_Dto`)

Il est également possible d'ajouter la référence d'une interface à cette configuration. Cette interface sera implémentée par la classe `Fields`. Vous pourrez ainsi la manipuler plus facilement. Si l'interface en question est suffixée par `<>`, alors elle sera considérée comme générique de la classe persistée.

Exemple :

La configuration suivante

```yaml
fieldsEnum: true
fieldsEnumInterface: topmodel.exemple.utils.IFieldEnum<>
```

Génèrera, dans la classe `Departement`, l'enum suivante :

```java
    public enum Fields implements IFieldEnum<Departement> {
         ID, //
         CODE_POSTAL, //
         LIBELLE
    }
```

## Générateur de resources

Le générateur de resources s'appuie sur les `Label` des propriétés, ainsi que sur les traductions récupérées dans le cadre de la configuration du [multilinguisme](/model/i18n.md).

Il suffit d'ajouter la configuration `resourcesPath` au générateur comme suit :

```yaml
jpa:
  - tags:
      - dto
    resourcesPath: resources/i18n/model # Chemin des fichiers de ressource générés.
```

Pour que, pour chaque module, soit généré les fichiers de resources dans les différentes langues configurées globalement.

Par défaut, les fichiers sont générés avec l'encodage Latin1, mais il est possible de les générer en UTF8 avec la propriété resourcesEncoding

```yaml
jpa:
  - tags:
      - dto
    resourcesPath: resources/i18n/model # Chemin des fichiers de ressource générés.
    resourcesEncoding: UTF8 # Encodage fichiers de ressource générés (Latin1 ou UTF8).
```

## Générateur de flow

Le générateur de data flow s'appuie sur `spring-batch`. Il permet de générer du code permettant de récupérer des données d'une source, appliquer éventuellement une transformation, puis les insérer dans une base de données. Les outils mis en oeuvre ont été sélectionnés pour leur capacité à traiter un grand nombre de données, avec les meilleures performances possibles.

> Il est recommandé de maîtriser le fonctionnement de `spring-batch` avant de tenter de générer des flows avec `TopModel`.

### Fichiers générés

#### Flow

Le générateur créé un fichier par dataFlow, comprenant :

- Reader
- Writer
- TruncateTasklet éventuellement
- Step
- Flow

La génération s'appuie sur spring-batch, mais aussi la librairie `spring-batch-bulk`, qui permet des performances exceptionnelles grâce à l'utilisation du bulk insert postgres (avec la commande `COPY`).

```xml
  <dependency>
    <groupId>io.github.klee-contrib</groupId>
    <artifactId>spring-batch-bulk</artifactId>
    <version>0.0.3</version>
  </dependency>
```

#### Reader

Le reader privilégié est le reader `JdbcCursorItemReaderBuilder`. Il permet d'obtenir les meilleures performances, et offre une meilleure flexibilité (choix de la source de données, requête).

Avec le mode `partial`, le reader n'est pas généré. Il faut donc fournir un `bean` dont le nom est `[Nom du flow]Reader` pour que le job fonctionne.

Il est par exemple possible de créer un `Reader` appelant une API.

##### Replace

Le truncate se fait avec la classe `TaskletQuery` de la librairie `spring-batch-bulk`. Nous aurions pu utiliser un `deleteAll` mais il est nettement moins performant que le `truncate`.

#### Processor

Si la classe source et la classe cible sont différentes, un processor est ajouté pour appeler le mapper de l'une vers l'autre

#### Writer

Les writers utilisent le `PgBulkWriter` de la librairie `spring-batch-bulk`. Il existe deux modes

##### Insert

Le writer copy directement les données dans la table cible. TopModel génère le mapping permettant de faire cette insertion.

##### Upsert

Le writer copy les données dans une table temporaire, puis recopie les données de table à table. En cas de conflit sur la clé primaire, un update est effectué. TopModel génère le mapping permettant de faire cette insertion.

#### Job

Le générateur créé un fichier de configuration de job par module. Ce job ordonnance les lancement des flow selon ce qui a été paramétré dans avec les mots clés `dependsOn`. Il import les configurations nécessaires à son bon fonctionnement.

### Limitations et mises en garde

- Ne fonctionne que de base à base. Pour créer un reader spécifique, utiliser le mode `partial`
- La base cible ne peut être qu'une base de données `Postgresql`
- Il est obligatoire de définir un dbSchema
- Multi-source non supporté
- Un mapper doit exister de la classe source vers la classe cible (sauf s'il s'agit de la même classe)
- Deux jobs ne peuvent pas dépendre l'un de l'autre s'ils ne sont pas dans le même module
- Prenons les flow A, B, C et D, avec
  - C dépend de A et B
  - D dépend de A
    alors D ne se lancera qu'après A et B (alors qu'en théorie il pourrait se lancer directement après A).

## Configuration

### Fichier de configuration

- `entitiesPath`

  Localisation des classses persistées du modèle, relatif au répertoire de génération.

  Le chemin des fichiers cibles sera calculé en remplaçant les `.` et le `:` par des `/` dans cette valeur, tandis que le nom du package des classes générées sera calculé en prenant ce qui est à droite du dernier `:` et en remplaçant tous les `/` par des `.`.

  _Templating_: `{module}`

  _Valeur par défaut_: `"javagen:{app}/entities/{module}"`

  _Variables par tag_: **oui** (plusieurs définition de classes pourraient être générées si un fichier à plusieurs tags)

- `daosPath`

  Localisation des DAOs, relative au répertoire de génération.

  Le chemin des fichiers cibles sera calculé en remplaçant les `.` et le `:` par des `/` dans cette valeur, tandis que le nom du package des classes générées sera calculé en prenant ce qui est à droite du dernier `:` et en remplaçant tous les `/` par des `.`.

  _Templating_: `{module}`

  _Variables par tag_: **oui** (plusieurs DAOs pourraient être générés si un fichier à plusieurs tags)

- `daosAbstract`

  Génération des DAO sous forme 'Abtract' à hériter pour l'utiliser dans le projet dans le projet avec :

  - le nom Abstract{classe.NamePascal}DAO
  - le fichier java sera mise à jour (écrasé) à chaque génération de code
  - l'annotation @NoRepositoryBean ajoutée (org.springframework.data.repository.NoRepositoryBean) permettant de ne pas considérer cette interface comme un DAO
    - il faut donc créer une interface qui en hérite dans le projet
  - le 'daosPath' peut être dans un répertoire de type 'javagen'

- `daosInterface`

  Permet de surcharger les interfaces par default des DAOS:

  - si UseJdbc, l'interface est org.springframework.data.repository.CrudRepository
  - si Reference, l'interface est org.springframework.data.repository.CrudRepository
  - si aucun des deux, l'interface est org.springframework.data.jpa.repository.JpaRepository
  - si daosInterface est précisée, les autres cas ne sont pas utilisés.

  Seul le nom de la classe est configurable, elle doit respecter le même pattern générique que JpaRespository et CrudRepository soit :

  - La classe de l'entité en premier
  - La classe de l'identifiant en second
  - {DaosInterface}<{classe.NamePascal}, {pk}>

- `dtosPath`

  Localisation des classes non persistées du modèle, relative au répertoire de génération.

  Le chemin des fichiers cibles sera calculé en remplaçant les `.` et le `:` par des `/` dans cette valeur, tandis que le nom du package des classes générées sera calculé en prenant ce qui est à droite du dernier `:` et en remplaçant tous les `/` par des `.`.

  _Templating_: `{module}`

  _Valeur par défaut_: `"javagen:{app}/dtos/{module}"`

  _Variables par tag_: **oui** (plusieurs définition de classes pourraient être générées si un fichier à plusieurs tags)

- `enumsPath`

  Localisation des classes d'enums, relative au répertoire de génération.

  Le chemin des fichiers cibles sera calculé en remplaçant les `.` et le `:` par des `/` dans cette valeur, tandis que le nom du package des classes générées sera calculé en prenant ce qui est à droite du dernier `:` et en remplaçant tous les `/` par des `.`.

  _Templating_: `{module}`

  _Valeur par défaut_: `"javagen:{app}/enums/{module}"`

  _Variables par tag_: **oui** (plusieurs définition de classes pourraient être générées si un fichier à plusieurs tags)

- `apiPath`

  Localisation du l'API générée (client ou serveur), relative au répertoire de génération.

  Le chemin des fichiers cibles sera calculé en remplaçant les `.` et le `:` par des `/` dans cette valeur, tandis que le nom du package des classes générées sera calculé en prenant ce qui est à droite du dernier `:` et en remplaçant tous les `/` par des `.`.

  _Templating_: `{module}`

  _Valeur par défaut_: `"javagen:{app}/api/{module}"`

  _Variables par tag_: **oui** (plusieurs clients/serveurs pourraient être générés si un fichier à plusieurs tags)

- `apiGeneration`

  Mode de génération de l'API (`"client"` ou `"server"`).

  _Variables par tag_: **oui** (la valeur de la variable doit être `"client"` ou `"server"`. le client et le serveur pourraient être générés si un fichier à plusieurs tags)

- `compositionConverterCanonicalName`
  Nom complet de la classe permettant de convertir les compositions stockées en json dans la bdd.
  _Templating_:

  - `{package}` : remplacé par le package de la classe composée
  - `{class}` : remplacé par le nom de la classe composée
    _Variables par tag_: **non**

- `resourcesPath`

  Localisation des ressources, relative au répertoire de génération.

  _Variables par tag_: **oui**

- `resourcesEncoding`

  Encodage des fichiers de ressources. Les valeurs possibles sont :

  - `Latin1` : valeur par défaut
  - `UTF8`

  _Variables par tag_: **non**

- `fieldsEnum`

  Option pour générer une enum des champs de certaines classes. Les valeurs possibles sont :

  - `None` : valeur par défaut, ne fait rien
  - `Persisted` : ajoute l'enum des champs sur les classes persistées
  - `Dto` : ajoute l'enum des champs sur les classes non persistées
  - `Persisted_Dto` : ajoute l'enum des champs sur toutes le classes

- `fieldsEnumInterface`

  Précise l'interface des fields enum générés.

  _Templating_: `<>` (remplace par `<NomDeLaClasse>`)

- `associationAdders`

  Option pour générer des méthodes d'ajouts pour les associations oneToMany et manyToMany. Ces méthodes permettent de synchroniser les objets ajoutés.

  _Valeur par défaut_: `false`

- `associationRemovers`

  Option pour générer des méthodes de suppression pour les associations oneToMany et manyToMany. Ces méthodes permettent de synchroniser les objets supprimés.

  _Valeur par défaut_: `false`

- `generatedHint`

  Option pour générer l'annotation @Generated("TopModel : https://github.com/klee-contrib/topmodel")

  _Valeur par défaut_: `true`

- `persistenceMode`

  Mode de génération de la persistence (`"javax"` ou `"jakarta"`).

  _Variables par tag_: **oui** (la valeur de la variable doit être `"javax"` ou `"jakarta"`)

- `mappersInClass`

  Indique s'il faut ajouter les mappers en tant méthode (`to...`) ou constructeur dans les classes qui les déclarent

  _Valeur par défaut_: `true`

- `identity`

  Options de génération de la séquence

  - `mode`

    Mode de génération de la persistence (`"none"` ou `"sequence"` ou `"identity"`).

    _Valeur par défaut_: `identity`

  - `increment`

    Incrément de la séquence générée.

  - `start`

    Début de la séquence générée.

### Exemple

Voici un exemple de configuration du générateur JPA

```yaml
jpa:
  - tags:
      - dto
      - entity
    outputDirectory: ./jpa/src/main/javagen # Dossier cible de la génération
    entitiesPath: topmodel/exemple/name/entities # Dossier cible des objets non persistés
    daosPath: topmodel/exemple/name/daos # Dossier cible des DAO
    dtosPath: topmodel/exemple/name/dtos # Dossier cible des objets non persistés
    enumsPath: topmodel/exemple/name/enums # Dossier cible des enums
    apiPath: topmodel/exemple/name/api # Dossier cible des API
    apiGeneration: Server # Mode de génération de l'API (serveur ou client)
    fieldsEnum: Persisted # Classes  dans lesquelles le générateur doit ajouter une enum des champs : jamais (None), dans les classes persistées (Persisted), dans les classes non persistées (Dto), ou les deux (Persisted_Dto)
    fieldsEnumInterface: topmodel.exemple.utils.IFieldEnum<> # Classe dont doivent hériter ces enum
```

## Snippets

### Domains

```yaml
---
domain:
  name: ID
  label: ID technique
  autoGeneratedValue: true
  asDomains:
    list: LIST
  java:
    type: Long
---
domain:
  name: MAIL
  asDomains:
    list: LIST
  label: Mail
  length: 100
  java:
    type: String
    annotations:
      - text: '@Email(message = "Le mail ''${validatedValue}'' n''est pas valide")'
        imports:
          - "jakarta.validation.constraints.Email"
---
domain:
  name: DATE_TIME
  label: Date
  asDomains:
    list: LIST
  java:
    type: LocalDateTime
    imports:
      - java.time.LocalDateTime
---
domain:
  name: TIME
  label: Heure
  asDomains:
    list: LIST
  java:
    type: LocalTime
    imports:
      - java.time.LocalTime
---
domain:
  name: DATE
  label: Date
  asDomains:
    list: LIST
  java:
    type: LocalDate
    imports:
      - java.time.LocalDate
---
domain:
  name: DATE_PAST
  label: Date
  asDomains:
    list: LIST
  java:
    type: LocalDate
    imports:
      - java.time.LocalDate
    annotations:
      - text: "@Past"
        imports:
          - "jakarta.validation.constraints.Past"
---
domain:
  name: DATE_CREATION
  label: Date
  asDomains:
    list: LIST
  java:
    type: LocalDate
    imports:
      - java.time.LocalDate
    annotations:
      - text: "@CreatedDate"
        imports:
          - org.springframework.data.annotation.CreatedDate
        target: Persisted
      - text: "@PastOrPresent"
        imports:
          - "jakarta.validation.constraints.PastOrPresent"
---
domain:
  name: DATE_MODIFICATION
  label: Date
  asDomains:
    list: LIST
  java:
    type: LocalDate
    imports:
      - java.time.LocalDate
    annotations:
      - text: "@LastModifiedDate"
        imports:
          - org.springframework.data.annotation.LastModifiedDate
        target: Persisted
      - text: "@PastOrPresent"
        imports:
          - "jakarta.validation.constraints.PastOrPresent"
---
domain:
  name: CREE_PAR
  label: Créé par
  scale: 50
  asDomains:
    list: LIST
  java:
    type: String
    annotations:
      - text: "@CreatedBy"
        imports:
          - org.springframework.data.annotation.CreatedBy
        target: Persisted
---
domain:
  name: MODIFIE_PAR
  label: Modifié par
  scale: 50
  asDomains:
    list: LIST
  java:
    type: String
    annotations:
      - text: "@LastModifiedBy"
        imports:
          - org.springframework.data.annotation.LastModifiedBy
        target: Persisted
---
domain:
  name: FILE_FORM
  mediaType: "multipart/form-data"
  label: Fichier
  bodyParam: true
  java:
    type: MultipartFile
    imports:
      - "org.springframework.web.multipart.MultipartFile"
---
domain:
  name: FILE
  mediaType: "multipart/form-data"
  label: Fichier
  bodyParam: true
  java:
    type: File
    imports:
      - "java.io.File"
---
domain:
  name: RESPONSE_ENTITY
  label: Response Entity
  java:
    type: ResponseEntity<{$0}>
    imports:
      - org.springframework.http.ResponseEntity
      - "{$1}"
---
domain:
  name: LIST
  label: Liste
  java:
    type: List<String>
    genericType: List<{T}>
    imports:
      - java.util.List
---
domain:
  name: POINT
  label: Point
  java:
    type: Point
    imports:
      - org.locationtech.jts.geom.Point
---
domain:
  name: POLYGONE
  label: Polygone
  java:
    type: Polygon
    imports:
      - org.locationtech.jts.geom.Polygon
---
domain:
  name: PAGE
  label: Date
  java:
    type: Page
    genericType: Page<{T}>
    imports:
      - "org.springframework.data.domain.Page"
---
domain:
  name: HTTP_RESPONSE
  label: Réponse Http
  java:
    type: ResponseEntity<Void>
    imports:
      - org.springframework.http.ResponseEntity
```

### Décorateurs

```yaml
---
decorator:
  name: DateCreation
  description: Entity Listener pour suivre les évènements de création
  java:
    annotations:
      - EntityListeners(AuditingEntityListener.class)
    imports:
      - org.springframework.data.jpa.domain.support.AuditingEntityListener
      - jakarta.persistence.EntityListeners
  properties:
    - name: DateCreation
      comment: Date de création de l'objet
      required: true
      domain: DATE_CREATION
      label: Date de création
---
decorator:
  name: DateModification
  description: Entity Listener pour suivre les évènements de modification
  java:
    annotations:
      - EntityListeners(AuditingEntityListener.class)
    imports:
      - org.springframework.data.jpa.domain.support.AuditingEntityListener
      - jakarta.persistence.EntityListeners
  properties:
    - name: DateModification
      comment: Date de création de l'objet
      required: true
      domain: DATE_MODIFICATION
      label: Date de modification
---
decorator:
  name: CreePar
  description: Entity Listener pour suivre les évènements de création
  java:
    annotations:
      - EntityListeners(AuditingEntityListener.class)
    imports:
      - org.springframework.data.jpa.domain.support.AuditingEntityListener
      - jakarta.persistence.EntityListeners
  properties:
    - name: CreePar
      comment: Auteur de la création de l'objet
      required: true
      domain: CREE_PAR
      label: Créateur
---
decorator:
  name: ModifiePar
  description: Entity Listener pour suivre les évènements de création
  java:
    annotations:
      - EntityListeners(AuditingEntityListener.class)
    imports:
      - org.springframework.data.jpa.domain.support.AuditingEntityListener
      - jakarta.persistence.EntityListeners
  properties:
    - name: ModifiePar
      comment: Auteur de la création de l'objet
      required: true
      domain: MODIFIE_PAR
      label: Créateur
---
decorator:
  name: HasAuthority
  description: Droit nécessaire pour pouvoir accéder au endpoint
  java:
    annotations:
      - '@PreAuthorize("hasAuthority(''{$0}'')")'
    imports:
      - org.springframework.security.access.prepost.PreAuthorize
```

### Configuration maven

Pour ajouter les sources du dossier `javagen` au build, vous pouvez utiliser la configuration suivante :

```xml
            <!-- Ajout des sources générées -->
            <plugin>
                <groupId>org.codehaus.mojo</groupId>
                <artifactId>build-helper-maven-plugin</artifactId>
                <executions>
                    <execution>
                        <id>add-source</id>
                        <phase>generate-sources</phase>
                        <goals>
                            <goal>add-source</goal>
                        </goals>
                        <configuration>
                            <sources>
                                <source>src/main/javagen</source>
                            </sources>
                        </configuration>
                    </execution>
                </executions>
            </plugin>
```
