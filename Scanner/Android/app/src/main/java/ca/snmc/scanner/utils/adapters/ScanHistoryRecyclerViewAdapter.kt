package ca.snmc.scanner.utils.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import ca.snmc.scanner.R
import ca.snmc.scanner.models.ScanHistoryItem
import kotlinx.android.synthetic.main.layout_scan_history_item.view.*

class ScanHistoryRecyclerViewAdapter : RecyclerView.Adapter<RecyclerView.ViewHolder>() {

    private var items : List<ScanHistoryItem> = ArrayList()

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RecyclerView.ViewHolder {
        return ScanHistoryItemViewHolder(
            LayoutInflater.from(parent.context).inflate(R.layout.layout_scan_history_item, parent, false)
        )
    }

    override fun onBindViewHolder(holder: RecyclerView.ViewHolder, position: Int) {

        when (holder) {

            is ScanHistoryItemViewHolder -> {
                holder.bind(items[position])
            }

        }

    }

    override fun getItemCount(): Int {
        return items.size
    }

    fun submitList(scanHistoryItemList : List<ScanHistoryItem>) {
        items = scanHistoryItemList
    }

    class ScanHistoryItemViewHolder constructor(
        itemView: View
    ) : RecyclerView.ViewHolder(itemView) {
        private val scanHistoryItemTextView : TextView = itemView.scan_history_item_text

        fun bind(scanHistoryItem: ScanHistoryItem) {
            scanHistoryItemTextView.text = scanHistoryItem.text
            scanHistoryItemTextView.setBackgroundResource(scanHistoryItem.backgroundResource)
        }
    }

}