import 'package:farm_marketplace_seller_app/app.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  testWidgets('Seller app boots and renders a MaterialApp', (WidgetTester tester) async {
    await tester.pumpWidget(const ProviderScope(child: SellerApp()));
    // Pump once for the initial async frame (loading spinner)
    await tester.pump();

    expect(find.byType(MaterialApp), findsOneWidget);
  });
}
