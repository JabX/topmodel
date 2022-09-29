﻿----
---- ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
----

-- =========================================================================================== 
--   Application Name	:	Exemple 
--   Script Name		:	01_tables.sql
--   Description		:	Script de création des tables.
-- =========================================================================================== 
/**
  * Création de la table PROFIL_DROITS_APPLI
 **/
create table PROFIL_DROITS_APPLI (
	ID_APPLI int8 not null,
	CODE_APPLI varchar(3) not null,
	constraint PK_PROFIL_DROITS_APPLI primary key (ID_APPLI,CODE_APPLI)
)
;

/**
  * Création de la table DROITS
 **/
create table DROITS (
	CODE varchar(3) not null,
	LIBELLE varchar(3) not null,
	constraint PK_DROITS primary key (CODE)
)
;

/**
  * Création de la table PROFIL
 **/
create table PROFIL (
	ID int8 generated by default as identity (start with 1000 increment 50) not null,
	CODE varchar(3),
	constraint PK_PROFIL primary key (ID)
)
;

/**
  * Création de la table SECTEUR
 **/
create table SECTEUR (
	SEC_ID int8 generated by default as identity (start with 1000 increment 50) not null,
	ID int8,
	constraint PK_SECTEUR primary key (SEC_ID)
)
;

/**
  * Création de la table TYPE_PROFIL
 **/
create table TYPE_PROFIL (
	CODE varchar(3) not null,
	LIBELLE varchar(3) not null,
	constraint PK_TYPE_PROFIL primary key (CODE)
)
;

/**
  * Création de la table TYPE_UTILISATEUR
 **/
create table TYPE_UTILISATEUR (
	TUT_CODE varchar(3) not null,
	TUT_LIBELLE varchar(3) not null,
	constraint PK_TYPE_UTILISATEUR primary key (TUT_CODE)
)
;

/**
  * Création de la table UTILISATEUR
 **/
create table UTILISATEUR (
	DATE_CREATION date,
	DATE_MODIFICATION date,
	ID int8 generated by default as identity (start with 1000 increment 50) not null,
	AGE numeric(20, 9),
	ID int8,
	EMAIL varchar(50),
	TUT_CODE varchar(3),
	ID_PARENT int8,
	constraint PK_UTILISATEUR primary key (ID)
)
;

