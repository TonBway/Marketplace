import 'package:flutter_riverpod/flutter_riverpod.dart';

class BagItem {
  const BagItem({
    required this.listing,
    required this.quantity,
  });

  final Map<String, dynamic> listing;
  final int quantity;

  String get listingId =>
      (listing['listingId'] ?? listing['listing_id'] ?? '').toString();

  BagItem copyWith({
    Map<String, dynamic>? listing,
    int? quantity,
  }) =>
      BagItem(
        listing: listing ?? this.listing,
        quantity: quantity ?? this.quantity,
      );
}

class BagNotifier extends StateNotifier<List<BagItem>> {
  BagNotifier() : super(const []);

  void addItem(Map<String, dynamic> listing, {int quantity = 1}) {
    final listingId = (listing['listingId'] ?? listing['listing_id'] ?? '').toString();
    if (listingId.isEmpty) return;

    final index = state.indexWhere((e) => e.listingId == listingId);
    if (index >= 0) {
      final existing = state[index];
      final updated = existing.copyWith(quantity: existing.quantity + quantity);
      state = [
        ...state.sublist(0, index),
        updated,
        ...state.sublist(index + 1),
      ];
      return;
    }

    state = [...state, BagItem(listing: listing, quantity: quantity)];
  }

  void removeItem(String listingId) {
    state = state.where((e) => e.listingId != listingId).toList();
  }

  void updateQuantity(String listingId, int quantity) {
    if (quantity < 1) return;
    state = state
        .map((e) => e.listingId == listingId ? e.copyWith(quantity: quantity) : e)
        .toList();
  }

  void clear() => state = const [];
}

final bagProvider = StateNotifierProvider<BagNotifier, List<BagItem>>(
  (ref) => BagNotifier(),
);
