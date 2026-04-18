import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';

class SellerProfileScreen extends ConsumerStatefulWidget {
  const SellerProfileScreen({super.key});

  @override
  ConsumerState<SellerProfileScreen> createState() =>
      _SellerProfileScreenState();
}

class _SellerProfileScreenState extends ConsumerState<SellerProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _businessCtrl = TextEditingController();
  final _descriptionCtrl = TextEditingController();

  bool _loading = true;
  bool _saving = false;
  bool _verified = false;

  int? _regionId;
  int? _districtId;
  String _contactMode = 'PHONE';

  List<Map<String, dynamic>> _regions = const [];
  List<Map<String, dynamic>> _districts = const [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _businessCtrl.dispose();
    _descriptionCtrl.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final regionsResp =
          await ref.read(apiClientProvider).dio.get('/api/reference/regions');
      final regions = (regionsResp.data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();

      setState(() => _regions = regions);

      try {
        final profileResp =
            await ref.read(apiClientProvider).dio.get('/api/seller/profile/me');
        final profile = (profileResp.data as Map).cast<String, dynamic>();

        _businessCtrl.text = profile['businessName']?.toString() ?? '';
        _descriptionCtrl.text = profile['description']?.toString() ?? '';
        _regionId = (profile['regionId'] as num?)?.toInt();
        _districtId = (profile['districtId'] as num?)?.toInt();
        _contactMode = profile['contactMode']?.toString() ?? 'PHONE';
        _verified = profile['isVerified'] == true;
      } catch (_) {
        // New seller without profile.
      }

      if (_regionId == null && _regions.isNotEmpty) {
        _regionId = (_regions.first['regionId'] as num).toInt();
      }

      if (_regionId != null) {
        await _loadDistricts(_regionId!);
        if (_districtId == null && _districts.isNotEmpty) {
          _districtId = (_districts.first['districtId'] as num).toInt();
        }
      }
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to load profile: $error')),
      );
    } finally {
      if (mounted) {
        setState(() => _loading = false);
      }
    }
  }

  Future<void> _loadDistricts(int regionId) async {
    final response = await ref.read(apiClientProvider).dio.get(
        '/api/reference/districts',
        queryParameters: {'regionId': regionId});

    setState(() {
      _districts = (response.data as List)
          .map((e) => (e as Map).cast<String, dynamic>())
          .toList();
    });
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    if (_regionId == null || _districtId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select region and district.')),
      );
      return;
    }

    setState(() => _saving = true);
    try {
      await ref.read(apiClientProvider).dio.put(
        '/api/seller/profile/me',
        data: {
          'businessName': _businessCtrl.text.trim(),
          'description': _descriptionCtrl.text.trim().isEmpty
              ? null
              : _descriptionCtrl.text.trim(),
          'regionId': _regionId,
          'districtId': _districtId,
          'contactMode': _contactMode,
          'profileImageUrl': null,
        },
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Profile saved successfully.')),
      );
      await _load();
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to save profile: $error')),
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
      return const Center(child: CircularProgressIndicator());
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(12),
        children: [
          if (_verified)
            Card(
              color: Colors.green.shade50,
              child: const ListTile(
                leading: Icon(Icons.verified, color: Colors.green),
                title: Text('Verified Seller'),
              ),
            ),
          Form(
            key: _formKey,
            child: Column(
              children: [
                TextFormField(
                  controller: _businessCtrl,
                  decoration: const InputDecoration(
                    labelText: 'Business Name',
                    border: OutlineInputBorder(),
                  ),
                  validator: (v) => (v == null || v.trim().isEmpty)
                      ? 'Business name is required'
                      : null,
                ),
                const SizedBox(height: 10),
                TextFormField(
                  controller: _descriptionCtrl,
                  minLines: 2,
                  maxLines: 4,
                  decoration: const InputDecoration(
                    labelText: 'Description',
                    border: OutlineInputBorder(),
                  ),
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
                        (r) => DropdownMenuItem<int>(
                          value: (r['regionId'] as num).toInt(),
                          child: Text(r['regionName'].toString()),
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
                    if (_districts.isNotEmpty) {
                      setState(() => _districtId =
                          (_districts.first['districtId'] as num).toInt());
                    }
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
                        (d) => DropdownMenuItem<int>(
                          value: (d['districtId'] as num).toInt(),
                          child: Text(d['districtName'].toString()),
                        ),
                      )
                      .toList(),
                  onChanged: (value) => setState(() => _districtId = value),
                ),
                const SizedBox(height: 10),
                DropdownButtonFormField<String>(
                  value: _contactMode,
                  decoration: const InputDecoration(
                    labelText: 'Preferred Contact',
                    border: OutlineInputBorder(),
                  ),
                  items: const [
                    DropdownMenuItem(value: 'PHONE', child: Text('Phone Call')),
                    DropdownMenuItem(value: 'SMS', child: Text('SMS')),
                    DropdownMenuItem(
                        value: 'APP_CHAT', child: Text('In-App Chat')),
                  ],
                  onChanged: (value) {
                    if (value != null) {
                      setState(() => _contactMode = value);
                    }
                  },
                ),
                const SizedBox(height: 14),
                SizedBox(
                  width: double.infinity,
                  child: FilledButton(
                    onPressed: _saving ? null : _save,
                    child: _saving
                        ? const SizedBox(
                            height: 18,
                            width: 18,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Text('Save Profile'),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
