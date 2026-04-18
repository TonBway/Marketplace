import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';

class CreateListingScreen extends ConsumerStatefulWidget {
  const CreateListingScreen({super.key});

  @override
  ConsumerState<CreateListingScreen> createState() =>
      _CreateListingScreenState();
}

class _CreateListingScreenState extends ConsumerState<CreateListingScreen> {
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

  @override
  void initState() {
    super.initState();
    _loadInitialData();
  }

  @override
  void dispose() {
    _titleCtrl.dispose();
    _descriptionCtrl.dispose();
    _priceCtrl.dispose();
    _quantityCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadInitialData() async {
    setState(() => _loading = true);
    try {
      final client = ref.read(apiClientProvider).dio;
      final responses = await Future.wait([
        client.get('/api/reference/categories'),
        client.get('/api/reference/units'),
        client.get('/api/reference/regions'),
      ]);

      final categories = (responses[0].data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();
      final units = (responses[1].data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();
      final regions = (responses[2].data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();

      setState(() {
        _categories = categories;
        _units = units;
        _regions = regions;

        if (_categories.isNotEmpty) {
          _categoryId = (_categories.first['id'] as num).toInt();
        }
        if (_units.isNotEmpty) {
          _unitId = (_units.first['id'] as num).toInt();
        }
        if (_regions.isNotEmpty) {
          _regionId = (_regions.first['regionId'] as num).toInt();
        }
      });

      if (_categoryId != null) {
        await _loadProductTypes(_categoryId!);
      }
      if (_regionId != null) {
        await _loadDistricts(_regionId!);
      }
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to load reference data: $error')),
      );
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  Future<void> _loadProductTypes(int categoryId) async {
    final response = await ref.read(apiClientProvider).dio.get(
      '/api/reference/product-types',
      queryParameters: {'categoryId': categoryId},
    );

    final productTypes = (response.data as List)
        .map((e) => (e as Map).cast<String, dynamic>())
        .toList();

    setState(() {
      _productTypes = productTypes;
      _productTypeId = _productTypes.isNotEmpty
          ? (_productTypes.first['productTypeId'] as num).toInt()
          : null;
    });
  }

  Future<void> _loadDistricts(int regionId) async {
    final response = await ref.read(apiClientProvider).dio.get(
      '/api/reference/districts',
      queryParameters: {'regionId': regionId},
    );

    final districts = (response.data as List)
        .map((e) => (e as Map).cast<String, dynamic>())
        .toList();

    setState(() {
      _districts = districts;
      _districtId = _districts.isNotEmpty
          ? (_districts.first['districtId'] as num).toInt()
          : null;
    });
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
      await ref.read(apiClientProvider).dio.post(
        '/api/listings',
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

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Listing created successfully.')),
      );
      Navigator.of(context).pop(true);
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to create listing: $error')),
      );
    } finally {
      if (mounted) {
        setState(() => _saving = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Create Listing')),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Create Listing')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(14),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              TextFormField(
                controller: _titleCtrl,
                decoration: const InputDecoration(
                  labelText: 'Title',
                  border: OutlineInputBorder(),
                ),
                validator: (value) => (value == null || value.trim().isEmpty)
                    ? 'Title is required'
                    : null,
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
                validator: (value) => (value == null || value.trim().isEmpty)
                    ? 'Description is required'
                    : null,
              ),
              const SizedBox(height: 10),
              DropdownButtonFormField<int>(
                value: _categoryId,
                decoration: const InputDecoration(
                  labelText: 'Category',
                  border: OutlineInputBorder(),
                ),
                items: _categories
                    .map(
                      (item) => DropdownMenuItem<int>(
                        value: (item['id'] as num).toInt(),
                        child: Text(item['name'].toString()),
                      ),
                    )
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
                    .map(
                      (item) => DropdownMenuItem<int>(
                        value: (item['productTypeId'] as num).toInt(),
                        child: Text(item['productTypeName'].toString()),
                      ),
                    )
                    .toList(),
                onChanged: (value) => setState(() => _productTypeId = value),
              ),
              if (_productTypes.isEmpty)
                const Padding(
                  padding: EdgeInsets.only(top: 6),
                  child: Text(
                    'No product types available for selected category. Please ask admin to configure product types.',
                    style: TextStyle(color: Colors.orange),
                  ),
                ),
              const SizedBox(height: 10),
              Row(
                children: [
                  Expanded(
                    child: TextFormField(
                      controller: _priceCtrl,
                      keyboardType:
                          const TextInputType.numberWithOptions(decimal: true),
                      decoration: const InputDecoration(
                        labelText: 'Price',
                        border: OutlineInputBorder(),
                      ),
                      validator: (value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'Price required';
                        }
                        if (double.tryParse(value.trim()) == null) {
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
                      keyboardType:
                          const TextInputType.numberWithOptions(decimal: true),
                      decoration: const InputDecoration(
                        labelText: 'Quantity',
                        border: OutlineInputBorder(),
                      ),
                      validator: (value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'Qty required';
                        }
                        if (double.tryParse(value.trim()) == null) {
                          return 'Invalid';
                        }
                        return null;
                      },
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 10),
              DropdownButtonFormField<int>(
                value: _unitId,
                decoration: const InputDecoration(
                  labelText: 'Unit',
                  border: OutlineInputBorder(),
                ),
                items: _units
                    .map(
                      (item) => DropdownMenuItem<int>(
                        value: (item['id'] as num).toInt(),
                        child: Text('${item['name']} (${item['code']})'),
                      ),
                    )
                    .toList(),
                onChanged: (value) => setState(() => _unitId = value),
              ),
              const SizedBox(height: 10),
              DropdownButtonFormField<int>(
                value: _regionId,
                decoration: const InputDecoration(
                  labelText: 'Region',
                  border: OutlineInputBorder(),
                ),
                items: _regions
                    .map(
                      (item) => DropdownMenuItem<int>(
                        value: (item['regionId'] as num).toInt(),
                        child: Text(item['regionName'].toString()),
                      ),
                    )
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
                    .map(
                      (item) => DropdownMenuItem<int>(
                        value: (item['districtId'] as num).toInt(),
                        child: Text(item['districtName'].toString()),
                      ),
                    )
                    .toList(),
                onChanged: (value) => setState(() => _districtId = value),
              ),
              const SizedBox(height: 10),
              SwitchListTile(
                contentPadding: EdgeInsets.zero,
                value: _isLivestock,
                title: const Text('Is Livestock'),
                subtitle: const Text('Toggle if this listing is livestock.'),
                onChanged: (value) => setState(() => _isLivestock = value),
              ),
              const SizedBox(height: 12),
              FilledButton(
                onPressed: _saving ? null : _submit,
                child: _saving
                    ? const SizedBox(
                        height: 18,
                        width: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Create Listing'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
