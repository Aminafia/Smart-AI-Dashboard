# Smart AI Dashboard — Implementation Sprint

Starting from commit `0c43eb3` (document content extraction).

Immediate goals:

1. Stabilize document retrieval and extraction.
2. Add document processing state.
3. Add focused backend/frontend tests.
4. Prepare the application for Docker and AWS deployment.

Cloud sequence:

- Dockerize API/worker
- Move document storage from local filesystem to S3
- Move background queue to SQS
- Deploy API/worker to ECS
- Add GitHub Actions CI/CD
- Add CloudWatch observability
