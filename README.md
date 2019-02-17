# PoC para o TCC de Arquitetura de Software Distribuído da PUC - Minas 

Prova de conceito para o Trabalho de Conclusão de Curso em Especialização em Arquitetura de Software Distribuído da PUC - Minas.

## Documento de Arquitetura

Este documento é composto das seguintes seções:

```
1. Objetivos do trabalho
2. Descrição geral da solução
3. Definição conceitual da solução
4. Modelagem e projeto arquitetural
5. Prova conceito / protótipo arquitetural
6. Avaliação arquitetural
7. APÊNDICE A - Ambiente de Desenvolvimento
```

Download do documento : <a href="https://github.com/RenStu/Coupon/blob/master/Solution%20Itens/TCC%20-%20Renan%20Stuchi%20-projeto%20arquitetural%202018.pdf">Download PDF</a>.
   

## Componentes Arquiteturais

Visçao geral dos componetes do sistema:

```
1. Server
   1. 1. Cluster Service Fabric (Obs.: PyGoogleImg é um serviço distribuído de scraping ao Google Images em Python)
   1. 2. Cluster Spiegel
   1. 3. Cluster CouchDB

2. Client
   2. 1. PouchDB
   2. 2. Microsoft Blazor (WebAssembly)
```

<img src="https://github.com/RenStu/Coupon/blob/master/Solution%20Itens/Coupon_Components.jpg?raw=true" width="500">


## Requisitos Funcionais

Foram selecionados três resquisitos funcionais para validar a PoC:

```
1. Register/Login
2. Create Offer
3. Request Coupon
```

Assista o vídeo : <a href="https://youtu.be/jjbMXgkrw1M">Requisitos Funcionais</a>.

## Requisitos Não Funcionais

Segue os resquisitos de qualidade validados na PoC:

```
1. Responsive Web Design
2. Security
3. Performance (Client - Side)
4. Resilience (Network)
5. Scalability
6. Disponibility
```

Assista o vídeo : <a href="https://youtu.be/E2hZHNWuFO8">Requisitos Não Funcionais</a>.

## Author

* **Renan Stuchi** - [RenStu](https://github.com/RenStu)