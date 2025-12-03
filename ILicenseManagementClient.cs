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
}
