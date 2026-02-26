# Neven.Axpo

## Code challenge for Axpo

Proposed solution uses Hosted service that periodically calls PowerService and transforms power trades 
to be exported into CSV format.

Location and time interval can be configured in hosted service configuration file.

## Projects in solution

#### Neven.Axpo.Domain

This class library project contains entities used in generating reports.

#### Neven.Axpo.Application

This class library project contains use case for getting power trades, transforming them into CSV friendly format, and exporting into CSV file.

#### Neven.Axpo.Infrastructure

This class library project contains implementation of services used in our use case.

#### Neven.Axpo.Service

This console application project runs hosted service that periodically triggers our use case to generate report in CSV file.

### Testing

There are two unit test projects that cover code in Neven.Axpo.Application and Neven.Axpo.Infrastructure.

There is also additional class library project that contains shared code to facilitate easier testing.



