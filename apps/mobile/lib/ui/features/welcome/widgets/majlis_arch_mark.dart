import 'package:flutter/material.dart';
import 'package:majlis/core/theme/majlis_colors.dart';

class MajlisArchMark extends StatelessWidget {
  const MajlisArchMark({
    required this.semanticLabel,
    this.size = 220,
    super.key,
  });

  final String semanticLabel;
  final double size;

  @override
  Widget build(BuildContext context) {
    return Semantics(
      image: true,
      label: semanticLabel,
      child: ExcludeSemantics(
        child: SizedBox.square(
          dimension: size,
          child: const CustomPaint(painter: _MajlisArchPainter()),
        ),
      ),
    );
  }
}

final class _MajlisArchPainter extends CustomPainter {
  const _MajlisArchPainter();

  @override
  void paint(Canvas canvas, Size size) {
    final outerArch = Path()
      ..moveTo(size.width * 0.16, size.height)
      ..lineTo(size.width * 0.16, size.height * 0.47)
      ..cubicTo(
        size.width * 0.16,
        size.height * 0.16,
        size.width * 0.36,
        size.height * 0.04,
        size.width * 0.5,
        size.height * 0.04,
      )
      ..cubicTo(
        size.width * 0.64,
        size.height * 0.04,
        size.width * 0.84,
        size.height * 0.16,
        size.width * 0.84,
        size.height * 0.47,
      )
      ..lineTo(size.width * 0.84, size.height)
      ..close();
    canvas.drawPath(outerArch, Paint()..color = MajlisColors.deepCoffee);

    final innerArch = Path()
      ..moveTo(size.width * 0.3, size.height)
      ..lineTo(size.width * 0.3, size.height * 0.5)
      ..cubicTo(
        size.width * 0.3,
        size.height * 0.28,
        size.width * 0.42,
        size.height * 0.19,
        size.width * 0.5,
        size.height * 0.19,
      )
      ..cubicTo(
        size.width * 0.58,
        size.height * 0.19,
        size.width * 0.7,
        size.height * 0.28,
        size.width * 0.7,
        size.height * 0.5,
      )
      ..lineTo(size.width * 0.7, size.height)
      ..close();
    canvas.drawPath(innerArch, Paint()..color = MajlisColors.sand);

    canvas.drawCircle(
      Offset(size.width * 0.5, size.height * 0.46),
      size.width * 0.09,
      Paint()..color = MajlisColors.majlisAmber,
    );
    canvas.drawLine(
      Offset(size.width * 0.24, size.height * 0.84),
      Offset(size.width * 0.76, size.height * 0.84),
      Paint()
        ..color = MajlisColors.palmGreen
        ..strokeCap = StrokeCap.round
        ..strokeWidth = size.width * 0.035,
    );
  }

  @override
  bool shouldRepaint(covariant _MajlisArchPainter oldDelegate) => false;
}
