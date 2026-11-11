package phy.maps
{
   import flash.display.Bitmap;
   import flash.display.BitmapData;
   import flash.geom.Point;
   import flash.geom.Rectangle;
   
   public class Tile extends Bitmap
   {
       
      
      private var _digable:Boolean;
      
      public function Tile(param1:BitmapData, param2:Boolean)
      {
         super(param1);
         this._digable = param2;
      }
      
      /**
       * True Gunny dig = punch transparent hole (see sky/back through terrain).
       * Never stamp crater/brink fills — that is the brown/yellow oval bug on Ruffle.
       * Punch every layer (fore + dead); ignore brink bitmap entirely.
       */
      public function Dig(param1:Point, param2:Bitmap, param3:Bitmap = null) : void
      {
         if(bitmapData == null)
         {
            return;
         }
         if(param2 && param2.bitmapData)
         {
            this.punchHole(param1,param2.bitmapData);
         }
         // intentionally ignore param3 brink — original BlendMode.ALPHA rim is unsafe on Ruffle
      }
      
      private function punchHole(center:Point, mask:BitmapData) : void
      {
         var ox:int = int(center.x - mask.width / 2);
         var oy:int = int(center.y - mask.height / 2);
         var mw:int = mask.width;
         var mh:int = mask.height;
         var mapW:int = bitmapData.width;
         var mapH:int = bitmapData.height;
         var x:int = 0;
         var y:int = 0;
         var dx:int = 0;
         var dy:int = 0;
         var ma:uint = 0;
         for(y = 0; y < mh; y++)
         {
            dy = oy + y;
            if(dy < 0 || dy >= mapH)
            {
               continue;
            }
            for(x = 0; x < mw; x++)
            {
               dx = ox + x;
               if(dx < 0 || dx >= mapW)
               {
                  continue;
               }
               ma = mask.getPixel32(x,y) >>> 24;
               if(ma > 16)
               {
                  bitmapData.setPixel32(dx,dy,0);
               }
            }
         }
      }
      
      private function paintBrink(center:Point, brink:BitmapData) : void
      {
         var ox:int = int(center.x - brink.width / 2);
         var oy:int = int(center.y - brink.height / 2);
         var bw:int = brink.width;
         var bh:int = brink.height;
         var mapW:int = bitmapData.width;
         var mapH:int = bitmapData.height;
         var x:int = 0;
         var y:int = 0;
         var dx:int = 0;
         var dy:int = 0;
         var bp:uint = 0;
         var ba:uint = 0;
         var mp:uint = 0;
         var ma:uint = 0;
         for(y = 0; y < bh; y++)
         {
            dy = oy + y;
            if(dy < 0 || dy >= mapH)
            {
               continue;
            }
            for(x = 0; x < bw; x++)
            {
               dx = ox + x;
               if(dx < 0 || dx >= mapW)
               {
                  continue;
               }
               bp = brink.getPixel32(x,y);
               ba = bp >>> 24;
               if(ba < 16)
               {
                  continue;
               }
               mp = bitmapData.getPixel32(dx,dy);
               ma = mp >>> 24;
               // Only paint rim onto remaining solid ground (keeps hole transparent → background shows).
               if(ma > 16)
               {
                  bitmapData.setPixel32(dx,dy,bp);
               }
            }
         }
      }
      
      public function DigFillRect(param1:Rectangle) : void
      {
         bitmapData.fillRect(param1,0);
      }
      
      public function GetAlpha(param1:int, param2:int) : uint
      {
         return bitmapData.getPixel32(param1,param2) >> 24 & 255;
      }
      
      public function dispose() : void
      {
         bitmapData.dispose();
      }
   }
}
