import 'package:flutter/material.dart';

class BuyerSearchScreen extends StatefulWidget {
  const BuyerSearchScreen({super.key});

  @override
  State<BuyerSearchScreen> createState() => _BuyerSearchScreenState();
}

class _BuyerSearchScreenState extends State<BuyerSearchScreen> {
  final _searchCtrl = TextEditingController();
  List<Map<String, dynamic>> _listings = [];
  bool _isLoading = false;

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  Future<void> _performSearch() async {
    setState(() => _isLoading = true);
    try {
      // TODO: Implement API call to search listings
      // For now, just show placeholder
      await Future.delayed(const Duration(seconds: 1));
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: SearchBar(
              controller: _searchCtrl,
              hintText: 'Search products...',
              onSubmitted: (_) => _performSearch(),
              leading: const Icon(Icons.search),
            ),
          ),
          if (_isLoading)
            const Expanded(
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_listings.isEmpty)
            const Expanded(
              child: Center(
                child: Text('Search for farm products'),
              ),
            )
          else
            Expanded(
              child: ListView.builder(
                itemCount: _listings.length,
                itemBuilder: (context, index) {
                  final listing = _listings[index];
                  return ListTile(
                    title: Text(listing['title'] ?? 'Product'),
                    subtitle: Text('${listing['price'] ?? 0} per ${listing['unit'] ?? 'unit'}'),
                  );
                },
              ),
            ),
        ],
      ),
    );
  }
}
