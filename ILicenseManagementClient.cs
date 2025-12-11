using LicenseManagement.Client.Models;
using LicenseManagement.Client.Requests;

namespace LicenseManagement.Client;

/// <summary>
/// Client interface for the License Management API.
/// </summary>
public interface ILicenseManagementClient
{
    #region Licenses

    /// <summary>
    /// Gets a license by product and computer.
    /// </summary>
    /// <param name="productId">The product ID (ULID with PRD_ prefix).</param>
    /// <param name="computerId">The computer ID (ULID with PC_ prefix).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The license if found, null otherwise.</returns>
    Task<License?> GetLicenseAsync(string productId, string computerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new trial license.
    /// </summary>
    /// <param name="request">The license creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created license.</returns>
    Task<License> CreateLicenseAsync(CreateLicenseRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a license (attach or detach a receipt).
    /// </summary>
    /// <param name="request">The license update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateLicenseAsync(UpdateLicenseRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Receipts

    /// <summary>
    /// Gets a receipt by code.
    /// </summary>
    /// <param name="code">The receipt code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The receipt if found, null otherwise.</returns>
    Task<Receipt?> GetReceiptAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new receipt.
    /// </summary>
    /// <param name="request">The receipt creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created receipt.</returns>
    Task<Receipt> CreateReceiptAsync(CreateReceiptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing receipt.
    /// </summary>
    /// <param name="request">The receipt update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateReceiptAsync(UpdateReceiptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a receipt code for a product and email combination.
    /// </summary>
    /// <param name="productName">The product name.</param>
    /// <param name="email">The buyer's email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated receipt code.</returns>
    Task<string> GenerateReceiptCodeAsync(string productName, string email, CancellationToken cancellationToken = default);

    #endregion

    #region Products

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    /// <param name="productId">The product ID (ULID with PRD_ prefix).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product if found, null otherwise.</returns>
    Task<Product?> GetProductAsync(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all products for the vendor.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of products.</returns>
    Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created product.</returns>
    Task<Product> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Computers

    /// <summary>
    /// Gets a computer by MAC address.
    /// </summary>
    /// <param name="macAddress">The MAC address or hardware ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The computer if found, null otherwise.</returns>
    Task<Computer?> GetComputerAsync(string macAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new computer.
    /// </summary>
    /// <param name="request">The computer registration request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registered computer.</returns>
    Task<Computer> RegisterComputerAsync(RegisterComputerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all computers registered to a receipt.
    /// </summary>
    /// <param name="receiptCode">The receipt code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of computers.</returns>
    Task<IEnumerable<Computer>> GetComputersAsync(string receiptCode, CancellationToken cancellationToken = default);

    #endregion

    #region Signing Keys

    /// <summary>
    /// Gets the vendor's public signing key.
    /// </summary>
    /// <param name="format">The key format: "xml" or "pem". Default is "xml".</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The public key in the requested format.</returns>
    Task<string> GetPublicKeyAsync(string format = "xml", CancellationToken cancellationToken = default);

    #endregion

    #region Webhooks

    /// <summary>
    /// Gets all webhooks for the vendor.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of webhooks.</returns>
    Task<IEnumerable<Webhook>> GetWebhooksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a webhook by ID.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The webhook if found, null otherwise.</returns>
    Task<Webhook?> GetWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new webhook.
    /// </summary>
    /// <param name="request">The webhook creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created webhook including the signing secret.</returns>
    Task<WebhookCreated> CreateWebhookAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a webhook.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="request">The webhook update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateWebhookAsync(string webhookId, UpdateWebhookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates the webhook secret.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="immediateRotation">If true, immediately invalidate the old secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new secret information.</returns>
    Task<WebhookSecretRotated> RotateWebhookSecretAsync(string webhookId, bool immediateRotation = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes secret rotation by removing the old secret.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CompleteSecretRotationAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets webhook delivery history.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="limit">Maximum number of deliveries to return (default 50).</param>
    /// <param name="offset">Offset for pagination (default 0).</param>
    /// <param name="status">Optional status filter (pending, success, failed, retrying, dead_letter).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of webhook deliveries.</returns>
    Task<IEnumerable<WebhookDelivery>> GetWebhookDeliveriesAsync(string webhookId, int limit = 50, int offset = 0, string? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific webhook delivery.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="deliveryId">The delivery ID (ULID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The delivery details.</returns>
    Task<WebhookDeliveryDetail?> GetWebhookDeliveryAsync(string webhookId, string deliveryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replays a failed webhook delivery.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="deliveryId">The delivery ID (ULID).</param>
    /// <param name="targetUrl">Optional override URL for the replay.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The replay delivery result.</returns>
    Task<WebhookDelivery> ReplayWebhookDeliveryAsync(string webhookId, string deliveryId, string? targetUrl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets webhook health status.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The webhook health status.</returns>
    Task<WebhookHealth> GetWebhookHealthAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets webhook delivery statistics.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The webhook statistics.</returns>
    Task<WebhookStats> GetWebhookStatsAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available webhook event types.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The available event types.</returns>
    Task<WebhookEventTypes> GetWebhookEventTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a test webhook to the specified endpoint.
    /// </summary>
    /// <param name="webhookId">The webhook ID (ULID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task TestWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

    #endregion
}
