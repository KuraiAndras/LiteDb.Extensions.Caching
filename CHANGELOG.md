# Changelog

## 2.5.1
- Update `AsyncKeyedLock`

## 2.5.0
- Use `AsyncKeyedLock` instead of semaphores in a concurrent dictionary

## 2.4.0
- Fix nullability attributes on `IMultiLevelCache`

## 2.3.0
- Add `ConnectionType` to options
- Update LiteDB dependency to `5.0.15`
- Support .NET 7.0

## 2.2.0
- Use `.ConfigureAwait(false)` everywhere