import 'package:flutter/material.dart';

class BuyerSentEnquiriesScreen extends StatefulWidget {
  const BuyerSentEnquiriesScreen({super.key});

  @override
  State<BuyerSentEnquiriesScreen> createState() =>
      _BuyerSentEnquiriesScreenState();
}

class _BuyerSentEnquiriesScreenState extends State<BuyerSentEnquiriesScreen> {
  List<Map<String, dynamic>> _enquiries = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadEnquiries();
  }

  Future<void> _loadEnquiries() async {
    setState(() => _isLoading = true);
    try {
      // TODO: Implement API call to load enquiries
      await Future.delayed(const Duration(seconds: 1));
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    if (_enquiries.isEmpty) {
      return Scaffold(
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.mail_outline, size: 64, color: Colors.grey),
              const SizedBox(height: 16),
              const Text('No enquiries sent'),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: () {
                  // TODO: Navigate to search
                },
                child: const Text('Browse Products'),
              ),
            ],
          ),
        ),
      );
    }

    return Scaffold(
      body: RefreshIndicator(
        onRefresh: _loadEnquiries,
        child: ListView.builder(
          itemCount: _enquiries.length,
          itemBuilder: (context, index) {
            final enquiry = _enquiries[index];
            return ListTile(
              title: Text(enquiry['productTitle'] ?? 'Product'),
              subtitle: Text('Status: ${enquiry['status'] ?? 'Pending'}'),
              trailing: const Icon(Icons.arrow_forward_ios),
              onTap: () {
                // TODO: View enquiry details
              },
            );
          },
        ),
      ),
    );
  }
}
