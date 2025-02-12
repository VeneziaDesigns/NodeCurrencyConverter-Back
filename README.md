# NodeCurrencyConverter es una API que consta de 4 capas con arquitectura DDD (Domain-Driven Design):

1. **DistributedServices**
2. **Bussiness**
3. **Domain**
4. **Infrastructure**

Se han utilizado las tecnoligias/principios:

- **DI** (Dependency Injection)
- **IoC** (Inversion of Control)
- **Cache** (Caching)
- **Logging**
- **Minimal API**

## Descripción del Ejercicio

El propósito de este proyecto es la creación de un algoritmo que encuentre el camino más corto a través de un grafo de nodos (divisas). La API devuelve el camino recorrido y los intercambios de divisas necesarios para llegar a la divisa de destino.

## Endpoints:

**GetAllCurrencies**
Devuelve una lista con las divisas disponibles para hacer la conversión

**GetAllCurrenciesExchanges**
Devuelve una lista con todos los cambios de divisa disponibles 

**GetShortestPath**
Devuelve una lista o un unico elemento (si hay una conversión directa), con el camino recorrido a traves del grafo de nodos

## Diagrama de Nodos

![NodeDiagram](NodeCurrencyConverter/NodeDiagram.png)
