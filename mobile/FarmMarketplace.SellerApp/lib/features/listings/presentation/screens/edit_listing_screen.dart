import 'dart:io';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:http_parser/http_parser.dart';
import 'package:image_picker/image_picker.dart';

import '../../../../core/providers.dart';

class EditListingScreen extends ConsumerStatefulWidget {
  final String listingId;

  const EditListingScreen({super.key, required this.listingId});

  @override
  ConsumerState<EditListingScreen> createState() => _EditListingScreenState();
}

class _EditListingScreenState extends ConsumerState<EditListingScreen> {
  static const _apiBaseUrl =
      String.fromEnvironment('API_BASE_URL', defaultValue: 'http://192.168.88.20:5000');

  final _formKey = GlobalKey<FormState>();
  final _titleCtrl = TextEditingController();
  final _descriptionCtrl = TextEditingController();
  final _priceCtrl = TextEditingController();
  final _quantityCtrl = TextEditingController();

  bool _loading = true;
  bool _saving = false;
  bool _isLivestock = false;

  int? _categoryId;
  int? _productTypeId;
  int? _unitId;
  int? _regionId;
  int? _districtId;

  List<Map<String, dynamic>> _categories = const [];
  List<Map<String, dynamic>> _productTypes = const [];
  List<Map<String, dynamic>> _units = const [];
  List<Map<String, dynamic>> _regions = const [];
  List<Map<String, dynamic>> _districts = const [];

  /// Existing images from the server
  List<Map<String, dynamic>> _existingImages = [];

  /// Newly picked images pending upload
  final List<XFile> _pendingImages = [];

  final ImagePicker _picker = ImagePicker();

  @override
  void initState() {
    super.initState();
    _loadAll();
  }

  @override
  void dispose() {
    _titleCtrl.dispose();
    _descriptionCtrl.dispose();
    _priceCtrl.dispose();
    _quantityCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadAll() async {
    setState(() => _loading = true);
    try {
      final client = ref.read(apiClientProvider).dio;

      // Fetch listing detail and reference data in parallel
      final results = await Future.wait([
        client.get('/api/listings/my/${widget.listingId}'),
        client.get('/api/reference/categories'),
        client.get('/api/reference/units'),
        client.get('/api/reference/regions'),
      ]);

      final detail = (results[0].data as Map).cast<String, dynamic>();
      final categories = (results[1].data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();
      final units = (results[2].data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();
      final regions = (results[3].data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();

      // Pre-populate text fields from detail
      _titleCtrl.text = detail['title']?.toString() ?? '';
      _descriptionCtrl.text = detail['description']?.toString() ?? '';
      _priceCtrl.text = detail['price']?.toString() ?? '';
      _quantityCtrl.text = detail['quantity']?.toString() ?? '';

      final existingCategoryId = (detail['categoryId'] as num?)?.toInt();
      final existingProductTypeId =
          (detail['productTypeId'] as num?)?.toInt();
      final existingUnitId = (detail['unitId'] as num?)?.toInt();
      final existingRegionId = (detail['regionId'] as num?)?.toInt();
      final existingDistrictId = (detail['districtId'] as num?)?.toInt();

      final images = (detail['images'] as List? ?? [])
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();

      setState(() {
        _categories = categories;
        _units = units;
        _regions = regions;
        _categoryId = existingCategoryId;
        _unitId = existingUnitId;
        _regionId = existingRegionId;
        _isLivestock = detail['isLivestock'] == true;
        _existingImages = images;
      });

      // Load product types and districts based on the saved category/region
      await Future.wait([
        if (existingCategoryId != null)
          _loadProductTypes(existingCategoryId,
              preselectId: existingProductTypeId),
        if (existingRegionId != null)
          _loadDistricts(existingRegionId, preselectId: existingDistrictId),
      ]);
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to load listing: $error')),
      );
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _loadProductTypes(int categoryId, {int? preselectId}) async {
    final response = await ref.read(apiClientProvider).dio.get(
      '/api/reference/product-types',
      queryParameters: {'categoryId': categoryId},
    );
    final productTypes = (response.data as List)
        .map((e) => (e as Map).cast<String, dynamic>())
        .toList();
    setState(() {
      _productTypes = productTypes;
      _productTypeId = preselectId ??
          (productTypes.isNotEmpty
              ? (productTypes.first['productTypeId'] as num).toInt()
              : null);
    });
  }

  Future<void> _loadDistricts(int regionId, {int? preselectId}) async {
    final response = await ref.read(apiClientProvider).dio.get(
      '/api/reference/districts',
      queryParameters: {'regionId': regionId},
    );
    final districts = (response.data as List)
        .map((e) => (e as Map).cast<String, dynamic>())
        .toList();
    setState(() {
      _districts = districts;
      _districtId = preselectId ??
          (districts.isNotEmpty
              ? (districts.first['districtId'] as num).toInt()
              : null);
    });
  }

  Future<void> _pickCameraImage() async {
    final totalCount = _existingImages.length + _pendingImages.length;
    if (totalCount >= 5) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Maximum 5 photos allowed per listing.')),
      );
      return;
    }
    final XFile? image = await _picker.pickImage(
      source: ImageSource.camera,
      imageQuality: 80,
      maxWidth: 1920,
    );
    if (image != null) {
      setState(() => _pendingImages.add(image));
    }
  }

  Future<void> _pickMultipleImages() async {
    final currentCount = _existingImages.length + _pendingImages.length;
    if (currentCount >= 5) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Maximum 5 photos allowed per listing.')),
      );
      return;
    }
    final remaining = 5 - currentCount;
    final images = await _picker.pickMultiImage(
      imageQuality: 80,
      maxWidth: 1920,
    );
    if (images.isNotEmpty) {
      final allowed = images.take(remaining).toList();
      setState(() => _pendingImages.addAll(allowed));
      if (images.length > remaining) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Only $remaining more photo(s) were added (5 max).')),
        );
      }
    }
  }

  void _showImageSourceSheet() {
    showModalBottomSheet(
      context: context,
      builder: (ctx) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.camera_alt),
              title: const Text('Take Photo'),
              onTap: () {
                Navigator.pop(ctx);
                _pickCameraImage();
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_library),
              title: const Text('Choose from Gallery'),
              onTap: () {
                Navigator.pop(ctx);
                _pickMultipleImages();
              },
            ),
          ],
        ),
      ),
    );
  }

  MediaType? _inferMediaType(XFile file) {
    final name = file.name.toLowerCase();
    final mime = file.mimeType?.toLowerCase();

    if (mime == 'image/jpeg' || mime == 'image/jpg') {
      return MediaType('image', 'jpeg');
    }
    if (mime == 'image/png') {
      return MediaType('image', 'png');
    }
    if (mime == 'image/webp') {
      return MediaType('image', 'webp');
    }
    if (mime == 'image/heic' || mime == 'image/heic-sequence') {
      return MediaType('image', 'heic');
    }
    if (mime == 'image/heif' || mime == 'image/heif-sequence') {
      return MediaType('image', 'heif');
    }

    if (name.endsWith('.jpg') || name.endsWith('.jpeg')) {
      return MediaType('image', 'jpeg');
    }
    if (name.endsWith('.png')) {
      return MediaType('image', 'png');
    }
    if (name.endsWith('.webp')) {
      return MediaType('image', 'webp');
    }
    if (name.endsWith('.heic')) {
      return MediaType('image', 'heic');
    }
    if (name.endsWith('.heif')) {
      return MediaType('image', 'heif');
    }
    return null;
  }

  Future<void> _uploadPendingImages(String listingId) async {
    final client = ref.read(apiClientProvider).dio;
    for (int i = 0; i < _pendingImages.length; i++) {
      final file = _pendingImages[i];
      final mediaType = _inferMediaType(file);
      if (mediaType == null) {
        throw Exception('Unsupported image format for ${file.name}. Use JPG, PNG, WEBP, HEIC or HEIF.');
      }

      final formData = FormData.fromMap({
        'file': await MultipartFile.fromFile(
          file.path,
          filename: file.name,
          contentType: mediaType,
        ),
        'isPrimary': i == 0 && _existingImages.isEmpty,
        'sortOrder': _existingImages.length + i,
      });
      await client.post(
        '/api/listings/$listingId/images/upload',
        data: formData,
      );
    }
  }

  Future<void> _deleteExistingImage(String imageId) async {
    if (imageId.isEmpty) return;
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Remove photo?'),
        content: const Text('This will permanently delete the photo from the listing.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Cancel')),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Remove', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    try {
      await ref.read(apiClientProvider).dio.delete(
        '/api/listings/${widget.listingId}/images/$imageId',
      );
      setState(() {
        _existingImages.removeWhere((img) => img['imageId']?.toString() == imageId);
      });
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to remove photo: $error')),
      );
    }
  }

  String _resolveImageUrl(String? rawUrl) {
    final value = (rawUrl ?? '').trim();
    if (value.isEmpty) return '';
    if (value.startsWith('http://') || value.startsWith('https://')) {
      return value;
    }
    final base = _apiBaseUrl.endsWith('/')
        ? _apiBaseUrl.substring(0, _apiBaseUrl.length - 1)
        : _apiBaseUrl;
    final path = value.startsWith('/') ? value : '/$value';
    return '$base$path';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    if (_categoryId == null ||
        _productTypeId == null ||
        _unitId == null ||
        _regionId == null ||
        _districtId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Please complete all required dropdowns.')),
      );
      return;
    }

    setState(() => _saving = true);
    try {
      final client = ref.read(apiClientProvider).dio;

      // Update listing details
      await client.put(
        '/api/listings/${widget.listingId}',
        data: {
          'title': _titleCtrl.text.trim(),
          'description': _descriptionCtrl.text.trim(),
          'categoryId': _categoryId,
          'productTypeId': _productTypeId,
          'price': double.parse(_priceCtrl.text.trim()),
          'quantity': double.parse(_quantityCtrl.text.trim()),
          'unitId': _unitId,
          'regionId': _regionId,
          'districtId': _districtId,
          'isLivestock': _isLivestock,
        },
      );

      // Upload any pending images
      if (_pendingImages.isNotEmpty) {
        await _uploadPendingImages(widget.listingId);
      }

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Listing updated successfully.')),
      );
      Navigator.of(context).pop(true);
    } catch (error) {
      if (!mounted) return;

      var message = 'Failed to update listing: $error';
      if (error is DioException) {
        final data = error.response?.data;
        if (data is Map && data['error'] != null) {
          message = data['error'].toString();
        }
      }

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(message)),
      );
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Edit Listing')),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text('Edit Listing'),
        actions: [
          if (_saving)
            const Padding(
              padding: EdgeInsets.all(16),
              child: SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            )
          else
            TextButton(
              onPressed: _submit,
              child: const Text('Save'),
            ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(14),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // ── Images Section ───────────────────────────────────────────
              _buildImagesSection(),
              const SizedBox(height: 14),
              const Divider(),
              const SizedBox(height: 6),

              // ── Basic Info ───────────────────────────────────────────────
              TextFormField(
                controller: _titleCtrl,
                decoration: const InputDecoration(
                  labelText: 'Title',
                  border: OutlineInputBorder(),
                ),
                validator: (v) =>
                    (v == null || v.trim().isEmpty) ? 'Title is required' : null,
              ),
              const SizedBox(height: 10),
              TextFormField(
                controller: _descriptionCtrl,
                minLines: 3,
                maxLines: 5,
                decoration: const InputDecoration(
                  labelText: 'Description',
                  border: OutlineInputBorder(),
                ),
                validator: (v) => (v == null || v.trim().isEmpty)
                    ? 'Description is required'
                    : null,
              ),
              const SizedBox(height: 10),

              // ── Category & Product Type ──────────────────────────────────
              DropdownButtonFormField<int>(
                value: _categoryId,
                decoration: const InputDecoration(
                  labelText: 'Category',
                  border: OutlineInputBorder(),
                ),
                items: _categories
                    .map((item) => DropdownMenuItem<int>(
                          value: (item['id'] as num).toInt(),
                          child: Text(item['name'].toString()),
                        ))
                    .toList(),
                onChanged: (value) async {
                  if (value == null) return;
                  setState(() {
                    _categoryId = value;
                    _productTypeId = null;
                  });
                  await _loadProductTypes(value);
                },
              ),
              const SizedBox(height: 10),
              DropdownButtonFormField<int>(
                value: _productTypeId,
                decoration: const InputDecoration(
                  labelText: 'Product Type',
                  border: OutlineInputBorder(),
                ),
                items: _productTypes
                    .map((item) => DropdownMenuItem<int>(
                          value: (item['productTypeId'] as num).toInt(),
                          child: Text(item['productTypeName'].toString()),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => _productTypeId = v),
              ),
              if (_productTypes.isEmpty)
                const Padding(
                  padding: EdgeInsets.only(top: 6),
                  child: Text(
                    'No product types available for selected category.',
                    style: TextStyle(color: Colors.orange),
                  ),
                ),
              const SizedBox(height: 10),

              // ── Price & Quantity ─────────────────────────────────────────
              Row(
                children: [
                  Expanded(
                    child: TextFormField(
                      controller: _priceCtrl,
                      keyboardType: const TextInputType.numberWithOptions(
                          decimal: true),
                      decoration: const InputDecoration(
                        labelText: 'Price',
                        border: OutlineInputBorder(),
                      ),
                      validator: (v) {
                        if (v == null || v.trim().isEmpty) {
                          return 'Required';
                        }
                        if (double.tryParse(v.trim()) == null) {
                          return 'Invalid';
                        }
                        return null;
                      },
                    ),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: TextFormField(
                      controller: _quantityCtrl,
                      keyboardType: const TextInputType.numberWithOptions(
                          decimal: true),
                      decoration: const InputDecoration(
                        labelText: 'Quantity',
                        border: OutlineInputBorder(),
                      ),
                      validator: (v) {
                        if (v == null || v.trim().isEmpty) {
                          return 'Required';
                        }
                        if (double.tryParse(v.trim()) == null) {
                          return 'Invalid';
                        }
                        return null;
                      },
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 10),

              // ── Unit ─────────────────────────────────────────────────────
              DropdownButtonFormField<int>(
                value: _unitId,
                decoration: const InputDecoration(
                  labelText: 'Unit',
                  border: OutlineInputBorder(),
                ),
                items: _units
                    .map((item) => DropdownMenuItem<int>(
                          value: (item['id'] as num).toInt(),
                          child:
                              Text('${item['name']} (${item['code']})'),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => _unitId = v),
              ),
              const SizedBox(height: 10),

              // ── Region & District ────────────────────────────────────────
              DropdownButtonFormField<int>(
                value: _regionId,
                decoration: const InputDecoration(
                  labelText: 'Region',
                  border: OutlineInputBorder(),
                ),
                items: _regions
                    .map((item) => DropdownMenuItem<int>(
                          value: (item['regionId'] as num).toInt(),
                          child: Text(item['regionName'].toString()),
                        ))
                    .toList(),
                onChanged: (value) async {
                  if (value == null) return;
                  setState(() {
                    _regionId = value;
                    _districtId = null;
                  });
                  await _loadDistricts(value);
                },
              ),
              const SizedBox(height: 10),
              DropdownButtonFormField<int>(
                value: _districtId,
                decoration: const InputDecoration(
                  labelText: 'District',
                  border: OutlineInputBorder(),
                ),
                items: _districts
                    .map((item) => DropdownMenuItem<int>(
                          value: (item['districtId'] as num).toInt(),
                          child: Text(item['districtName'].toString()),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => _districtId = v),
              ),
              const SizedBox(height: 16),

              FilledButton(
                onPressed: _saving ? null : _submit,
                child: _saving
                    ? const SizedBox(
                        height: 18,
                        width: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Save Changes'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildImagesSection() {
    final totalCount = _existingImages.length + _pendingImages.length;
    final atLimit = totalCount >= 5;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              'Photos ($totalCount / 5)',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            TextButton.icon(
              onPressed: atLimit ? null : _showImageSourceSheet,
              icon: const Icon(Icons.add_a_photo, size: 18),
              label: Text(atLimit ? 'Limit Reached' : 'Add Photo'),
            ),
          ],
        ),
        if (totalCount == 0)
          Container(
            height: 110,
            decoration: BoxDecoration(
              border: Border.all(color: Colors.grey.shade300),
              borderRadius: BorderRadius.circular(8),
              color: Colors.grey.shade50,
            ),
            child: const Center(
              child: Text(
                'No photos yet. Tap Add Photo to upload images.',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.grey),
              ),
            ),
          )
        else
          SizedBox(
            height: 110,
            child: ListView(
              scrollDirection: Axis.horizontal,
              children: [
                // Existing server images
                ..._existingImages.asMap().entries.map((entry) {
                  final img = entry.value;
                  final url = _resolveImageUrl(img['imageUrl']?.toString());
                  final isPrimary = img['isPrimary'] == true;
                  final imageId = img['imageId']?.toString() ?? '';
                  return _ExistingImageTile(
                    url: url,
                    isPrimary: isPrimary,
                    onRemove: () => _deleteExistingImage(imageId),
                  );
                }),
                // Pending local images
                ..._pendingImages.asMap().entries.map((entry) {
                  final index = entry.key;
                  final file = entry.value;
                  return _PendingImageTile(
                    file: file,
                    onRemove: () =>
                        setState(() => _pendingImages.removeAt(index)),
                  );
                }),
              ],
            ),
          ),
        if (_pendingImages.isNotEmpty)
          Padding(
            padding: const EdgeInsets.only(top: 6),
            child: Text(
              '${_pendingImages.length} photo(s) will be uploaded on save.',
              style: TextStyle(
                  color: Theme.of(context).colorScheme.primary, fontSize: 12),
            ),
          ),
      ],
    );
  }
}

// ─── Helper Widgets ─────────────────────────────────────────────────────────

class _ExistingImageTile extends StatelessWidget {
  final String url;
  final bool isPrimary;
  final VoidCallback onRemove;

  const _ExistingImageTile({required this.url, required this.isPrimary, required this.onRemove});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: Stack(
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: Image.network(
              url,
              width: 100,
              height: 100,
              fit: BoxFit.cover,
              errorBuilder: (_, __, ___) => Container(
                width: 100,
                height: 100,
                color: Colors.grey.shade200,
                child: const Icon(Icons.broken_image, color: Colors.grey),
              ),
            ),
          ),
          Positioned(
            top: 2,
            right: 2,
            child: GestureDetector(
              onTap: onRemove,
              child: Container(
                decoration: const BoxDecoration(
                  color: Colors.black54,
                  shape: BoxShape.circle,
                ),
                child: const Icon(Icons.close, color: Colors.white, size: 18),
              ),
            ),
          ),
          if (isPrimary)
            Positioned(
              bottom: 4,
              left: 4,
              child: Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 4, vertical: 2),
                decoration: BoxDecoration(
                  color: Colors.green,
                  borderRadius: BorderRadius.circular(4),
                ),
                child: const Text(
                  'Primary',
                  style: TextStyle(color: Colors.white, fontSize: 10),
                ),
              ),
            ),
        ],
      ),
    );
  }
}

class _PendingImageTile extends StatelessWidget {
  final XFile file;
  final VoidCallback onRemove;

  const _PendingImageTile({required this.file, required this.onRemove});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: Stack(
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: Image.file(
              File(file.path),
              width: 100,
              height: 100,
              fit: BoxFit.cover,
            ),
          ),
          Positioned(
            top: 2,
            right: 2,
            child: GestureDetector(
              onTap: onRemove,
              child: Container(
                decoration: const BoxDecoration(
                  color: Colors.black54,
                  shape: BoxShape.circle,
                ),
                child: const Icon(Icons.close, color: Colors.white, size: 18),
              ),
            ),
          ),
          Positioned(
            bottom: 4,
            left: 4,
            child: Container(
              padding:
                  const EdgeInsets.symmetric(horizontal: 4, vertical: 2),
              decoration: BoxDecoration(
                color: Colors.blue,
                borderRadius: BorderRadius.circular(4),
              ),
              child: const Text(
                'New',
                style: TextStyle(color: Colors.white, fontSize: 10),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
