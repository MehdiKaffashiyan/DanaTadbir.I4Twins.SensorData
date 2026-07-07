# i4Twins Sensor Data Ingestion & Aggregation Service

This project is a .NET backend service for ingesting, cleaning, deduplicating, and aggregating messy sensor data from a JSONL input source. It is designed to keep domain logic independent from infrastructure concerns while providing a reliable ingestion pipeline, a clear count report, and a maintainable testing strategy.

## Overview

The service processes sensor readings, filters invalid records, removes duplicates, and stores cleaned readings in a chosen data store. It also exposes time-based aggregation capabilities and generates a count report to summarize the ingestion run.

The input data is intentionally messy and may include:

- duplicate records
- out-of-order records
- invalid records
- empty or malformed lines

Handling these cases is part of the core task, and the policies used for validation, deduplication, and aggregation are documented below.

## Architecture

The solution follows **Clean Architecture**.

This architecture was chosen because it keeps the domain logic independent from infrastructure details such as file access, database access, and API hosting. That separation makes the core business rules easier to test, easier to reason about, and easier to evolve if the storage technology or delivery mechanism changes later.

### Domain Layer

The domain layer contains the business rules and model definitions, including:

- sensor reading entities
- validation rules
- duplicate detection rules
- aggregation-related abstractions
- repository interfaces

The domain layer does not depend on the file system, database libraries, or web framework details.

### Infrastructure Layer

The infrastructure layer contains the technical implementations, such as:

- file reading for the JSONL input
- data store persistence
- query execution
- repository implementations
- mapping between domain objects and storage models

### Application Layer

The application layer coordinates the workflow:

- ingestion
- cleaning
- deduplication
- aggregation
- count report generation
- API exposure if needed

## Storage Choice

The cleaned readings are persisted in **InfluxDB**.

### Why InfluxDB?

InfluxDB was chosen because:

- the problem is time-series oriented
- sensor readings are naturally indexed by timestamp
- aggregation over time ranges is a core requirement
- it supports efficient query patterns for sensor telemetry
- it fits the nature of the data better than a generic relational model for this use case

### Trade-offs

Using InfluxDB improves time-series aggregation and query ergonomics, but it also means:

- the repository logic must handle time-series-specific query patterns
- the application depends on InfluxDB availability during runtime and tests
- repository testing is better suited to integration tests than pure unit tests

## Ingestion and Cleaning

The ingestion pipeline reads the source data, validates records, removes duplicates, and stores only clean readings.

### Validation Policy

A record is rejected if it is malformed or does not satisfy the required structure and data constraints.

Invalid records are counted separately in the final report.

### Deduplication Policy

Two readings are considered identical when their `(deviceId, metric, ts, seq)` quadruple is equal.

This policy was chosen because:

- it matches the task requirement exactly
- it avoids accidental merging of distinct readings that happen to share partial fields
- it provides deterministic duplicate detection

### Handling Duplicates

When a duplicate reading is encountered:

- the first valid occurrence is kept
- later identical records are ignored
- duplicates are counted in the report

This policy is simple, deterministic, and efficient. It also preserves the first observed valid reading, which is a reasonable choice when the input stream may contain repeated events.

### Out-of-Order Data

The input may contain out-of-order readings.

The ingestion process does not require the source data to be pre-sorted. Out-of-order records are accepted if valid and unique. Time-based aggregation is performed using the timestamp field, so record order in the input does not affect correctness.

## Aggregation

The service exposes time-based aggregation over sensor readings.

Aggregation is performed over the cleaned dataset, not the raw input. This ensures that invalid and duplicate data do not affect the results.

The aggregation design prioritizes correctness and predictability over unnecessary complexity.

## Count Report

After ingestion, the system generates a count report that summarizes the processing result.

The report includes:

- total lines read
- readings stored
- duplicates removed
- invalid records rejected

This report is intended to provide quick visibility into the quality of the input data and the effect of the cleaning pipeline.

## Performance Considerations

The solution is designed to be reasonable in both time complexity and memory usage.

### Time Complexity

The ingestion pipeline uses a single-pass style workflow where possible, so each input record is processed once. Duplicate detection is designed to be efficient so that lookup time remains practical even as the input grows.

### Memory Usage

The deduplication approach keeps a set of seen reading keys during ingestion. This increases memory usage, but it enables fast duplicate detection.

### Trade-offs

The main trade-off is between speed and memory:

- keeping seen keys in memory improves deduplication performance
- however, it uses additional RAM
- this is acceptable for the expected task scope, while still being simple and reliable

If the dataset or deployment scenario later grows beyond a single process, the deduplication strategy can be moved to a distributed store.

## Testing Strategy

Tests focus on the most important business behavior:

- deduplication
- validation
- aggregation
- count report behavior
- repository behavior where appropriate

### Unit Tests

Unit tests are used for domain logic that can be isolated from infrastructure. This includes:

- validation rules
- duplicate detection policy
- aggregation behavior
- report calculation logic

### Integration Tests

Repository-level behavior is validated with integration tests, especially where the storage engine or SDK makes pure unit mocking impractical.

This is important because some external client APIs are not easy to mock cleanly, and realistic repository verification gives stronger confidence in the actual persistence and query path.

## Key Decisions and Trade-offs

The main decisions in this solution are:

- using Clean Architecture to separate business logic from technical details
- choosing InfluxDB for time-series storage and aggregation
- keeping duplicate handling deterministic by using the full `(deviceId, metric, ts, seq)` key
- accepting out-of-order input without requiring pre-sorting
- treating invalid records as rejected rather than partially repaired
- favoring a simple and efficient single-pass ingestion design

These decisions were made to keep the solution understandable, testable, and aligned with the requirements.

## AI Usage Disclosure

AI tools were used in the preparation of this project’s documentation and implementation support.

### How AI Was Used

- drafting and improving the README structure
- refining wording for technical clarity
- helping organize architecture and trade-off explanations
- assisting with implementation reasoning and test strategy discussion

### What Was Not Delegated to AI

- final responsibility for design decisions
- validation of task requirements
- project-specific implementation choices
- repository submission and review

The project content remains authored and reviewed by the developer, with AI used as a supportive tool.

## Run Instructions

1. Configure the application settings for the target environment.
2. Build the solution.
3. Run the application.
4. Verify that ingestion completes successfully and that the count report is generated.

## Test Instructions

1. Run the unit test suite for domain behavior.
2. Run the integration test suite for repository and storage behavior.
3. Confirm that duplicate handling, validation, and aggregation behave as expected.
4. Verify that the count report matches the input data characteristics.

## Deliverables

The final deliverable for this task is:

- the Git repository address
- the README file
